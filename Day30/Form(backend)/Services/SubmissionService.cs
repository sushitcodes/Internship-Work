using Form.DTOs;
using Form.Entities;
using Form.Interface;
using Form.Interfaces;
namespace Form.Services;

public class SubmissionService(
    ISubmissionRepository _repository,
    IFileStorageService _fileStorage,
    IUserProfileRepository _profileRepository,
    IEnrollmentService _enrollmentService,
    INotificationService _notificationService,
    IClassRoomRepository _classRoomRepository,
    ILogger<SubmissionService> _logger) : ISubmissionService
{
    public async Task<SubmissionDto> CreateSubmissionAsync(CreateSubmissionRequest request)
    {
        // 1. Cheap checks first: nothing is written yet.
        ValidateFile(request.File);

        var classRoomName = await _classRoomRepository.GetNameAsync(request.ClassRoomId)
            ?? throw new InvalidOperationException("Selected class was not found.");

        var profile = await _profileRepository.GetByMemberNumberAsync(request.RollNo)
            ?? throw new InvalidOperationException($"No student found with roll number {request.RollNo}.");

        // A student may only submit for THEMSELVES. Staff and admins may submit for anyone.
        if (!request.CreatedByStaff && profile.UserId != request.CreatedByUserId)
            throw new InvalidOperationException("You can only submit using your own roll number.");

        // 2. Enrollment BEFORE the file and the row exist, so a failure leaves nothing behind.
        await _enrollmentService.EnsureEnrolledAsync(profile.UserId, request.ClassRoomId);

        // 3. File, then row. If the row fails, remove the file again.
        var fileUrl = await _fileStorage.SaveFileAsync(request.File);
        Submission saved;
        try
        {
            saved = await _repository.AddAsync(new Submission
            {
                FullName = request.FullName,
                ClassRoomId = request.ClassRoomId,
                RollNo = request.RollNo,
                FileUrl = fileUrl,
                CreatedByUserId = request.CreatedByUserId,
            });
        }
        catch
        {
            await _fileStorage.DeleteFileAsync(fileUrl);
            throw;
        }

        // 4. Notifications are best effort: the submission is already saved.
        try
        {
            var teacherUserIds = await _classRoomRepository.GetTeacherUserIdsAsync(request.ClassRoomId);
            if (teacherUserIds.Count > 0)
            {
                await _notificationService.NotifyNewSubmissionAsync(
                    recipientUserIds: teacherUserIds,
                    studentName: request.FullName,
                    assignmentTitle: classRoomName,
                    submissionId: saved.Id);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Submission {SubmissionId} saved but notifying teachers failed", saved.Id);
        }

        var dto = MapToDto(saved);
        dto.ClassRoomName = classRoomName;   // AddAsync does not load the ClassRoom navigation
        return dto;
    }

    private static SubmissionDto MapToDto(Submission s,string? avatarUrl = null) => new()
    {
        Id = s.Id,
        FullName = s.FullName,
        ClassRoomId = s.ClassRoomId,
        ClassRoomName = s.ClassRoom?.Name ?? string.Empty,
        RollNo = s.RollNo,
        FileUrl = s.FileUrl,
        CreatedAt = s.CreatedAt,
        SubmitterAvatarUrl=avatarUrl,
        CreatedByUserId =s.CreatedByUserId,
    };

    public async Task<SubmissionDto?> GetByIdAsync(Guid id)
    {
        var submission = await _repository.GetByIdAsync(id);
        if (submission is null) return null;
        string? avatar = null;
        if(submission.CreatedByUserId.HasValue)
        {
            var profile = await _profileRepository.GetByUserIdAsync(submission.CreatedByUserId.Value);
            avatar = profile?.AvatarUrl;
        }
        return MapToDto(submission,avatar);
    }

    public async Task<int> GetCountAsync(Guid? createdByUserId = null) => await _repository.GetCountAsync(createdByUserId);

    public async Task<bool> DeleteAsync(Guid id) => await _repository.DeleteAsync(id);

    public async Task<PagedResult<SubmissionDto>> GetPagedAsync(int page, int pageSize, string? search, Guid? createdByUserId = null)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(page, pageSize, search, createdByUserId);

        var userIds = items
            .Where(s => s.CreatedByUserId.HasValue)
            .Select(s => s.CreatedByUserId!.Value)
            .Distinct()
            .ToList();
        var profiles = await _profileRepository.GetByUserIdsAsync(userIds);
        var avatarByUserId = profiles.ToDictionary(p => p.UserId, p => p.AvatarUrl);

        var dtos = items.Select(s =>
        {
            string? avatar = null;
            if (s.CreatedByUserId.HasValue)
                avatarByUserId.TryGetValue(s.CreatedByUserId.Value, out avatar);

            return MapToDto(s, avatar);
        }).ToList();
        return new PagedResult<SubmissionDto>
        {
            Items = dtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<SubmissionDto?> UpdateAsync(Guid id, UpdateSubmissionRequest request)
    {
        string? fileUrl = null;
        if (request.File is not null)
        {
            ValidateFile(request.File);
            fileUrl = await _fileStorage.SaveFileAsync(request.File);
        }

        var updated = new Submission
        {
            FullName = request.FullName,
            ClassRoomId = request.ClassRoomId,  // Added: Update ClassRoomId
            RollNo = request.RollNo,            // Added: Update RollNo
            FileUrl = fileUrl ?? string.Empty,  // empty signals "no new file" to the repository
            // REMOVED: Email, Phone, and Education
        };

        var result = await _repository.UpdateAsync(id, updated);
        if (result is null) return null;

        string? avatar = null;
        if (result.CreatedByUserId.HasValue)
        {
            var profile = await _profileRepository.GetByUserIdAsync(result.CreatedByUserId.Value);
            avatar = profile?.AvatarUrl;
        }

        return MapToDto(result, avatar);
    }

    private static void ValidateFile(IFormFile file)
    {
        var allowedExtensions = new[] { ".pdf", ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            throw new InvalidOperationException("File type not allowed. Use PDF, JPG, or PNG.");

        const long maxSizeBytes = 5 * 1024 * 1024; // 5 MB
        if (file.Length > maxSizeBytes)
            throw new InvalidOperationException("File too large. Max size is 5MB.");
    }
}