using Form.DTOs;
using Form.Interfaces;
using Form.Entities;
using static Form.DTOs.ClassRoomDtos;

namespace Form.Services;

public class SubmissionService : ISubmissionService
{
    private readonly ISubmissionRepository _repository;
    private readonly IFileStorageService _fileStorage;
    private readonly IUserProfileRepository _profileRepository; 
    private readonly IEnrollmentService _enrollmentService;

    public SubmissionService(ISubmissionRepository repository, IFileStorageService fileStorage, IUserProfileRepository profileRepository,
        IEnrollmentService enrollmentService)
    {
                _repository = repository;
        _fileStorage = fileStorage;
        _profileRepository = profileRepository;
        _enrollmentService = enrollmentService;

    }

    public async Task<SubmissionDto> CreateSubmissionAsync(CreateSubmissionRequest request)
    {
        ValidateFile(request.File);
        var fileUrl = await _fileStorage.SaveFileAsync(request.File);
        // Roll number must correspond to a REAL person's MemberNumber — if
        // it doesn't, this is almost certainly a typo, and letting the
        // submission through silently would create an orphaned record
        // nothing can ever link back to a real student. Fail loudly instead.
        var profile = await _profileRepository.GetByMemberNumberAsync(request.RollNo);
        if (profile is null)
            throw new InvalidOperationException($"No student found with roll number {request.RollNo}.");
        var submission = new Submission
        {
            FullName = request.FullName,
            ClassRoomId = request.ClassRoomId,
            RollNo = request.RollNo,
            FileUrl = fileUrl,
            CreatedByUserId = request.CreatedByUserId,
        };

        // TODO — enrollment linking: look up UserProfile where MemberNumber ==
        // request.RollNo, get its UserId, then ensure an Enrollment exists for
        // (UserId, request.ClassRoomId) — creating one if it doesn't. Holding
        // off writing this until I see your Enrollment repository/service, so
        // I don't guess a shape that conflicts with what already exists.

        var saved = await _repository.AddAsync(submission);
        await _enrollmentService.EnsureEnrolledAsync(profile.UserId, request.ClassRoomId);

        return MapToDto(saved);
    }

    private static SubmissionDto MapToDto(Submission s) => new()
    {
        Id = s.Id,
        FullName = s.FullName,
        ClassRoomId = s.ClassRoomId,
        ClassRoomName = s.ClassRoom?.Name ?? string.Empty,
        RollNo = s.RollNo,
        FileUrl = s.FileUrl,
        CreatedAt = s.CreatedAt,
    };

    public async Task<SubmissionDto?> GetByIdAsync(Guid id)
    {
        var submission = await _repository.GetByIdAsync(id);
        return submission is null ? null : MapToDto(submission);
    }

    public async Task<int> GetCountAsync() => await _repository.GetCountAsync();

    public async Task<bool> DeleteAsync(Guid id) => await _repository.DeleteAsync(id);

    public async Task<PagedResult<SubmissionDto>> GetPagedAsync(int page, int pageSize, string? search)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(page, pageSize, search);
        return new PagedResult<SubmissionDto>
        {
            Items = items.Select(MapToDto).ToList(),
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
        return result is null ? null : MapToDto(result);
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