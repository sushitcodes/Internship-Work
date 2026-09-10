using Form.DTOs;
using Form.Entities;
using Form.Interfaces;
namespace Form.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _repository;
    private readonly IUserRepository _userRepository;
    private readonly IUserProfileRepository _profileRepository;

    public EnrollmentService(
        IEnrollmentRepository repository,
        IUserRepository userRepository,
        IUserProfileRepository profileRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
        _profileRepository = profileRepository;
    }

    // ── The single source of truth for "one class at a time" ──
    private async Task EnsureStudentNotInAnotherClassAsync(Guid studentUserId, Guid classRoomId)
    {
        var existing = await _repository.GetByStudentUserIdAsync(studentUserId);
        if (existing is not null && existing.ClassRoomId != classRoomId)
            throw new InvalidOperationException(
                $"Student is already enrolled in class '{existing.ClassRoom.Name}'. " +
                "Remove them from that class first.");
    }

    public async Task<EnrollmentDto> EnrollAsync(CreateEnrollmentRequest request)
    {
        if (await _repository.ExistsAsync(request.StudentUserId, request.ClassRoomId))
            throw new InvalidOperationException("This student is already enrolled in this class.");

        await EnsureStudentNotInAnotherClassAsync(request.StudentUserId, request.ClassRoomId);

        var student = await _userRepository.GetByIdAsync(request.StudentUserId)
            ?? throw new InvalidOperationException("Student not found.");

        var saved = await _repository.AddAsync(new Enrollment
        {
            StudentUserId = request.StudentUserId,
            ClassRoomId = request.ClassRoomId,
        });

        var profile = await _profileRepository.GetByUserIdAsync(student.Id);

        return new EnrollmentDto
        {
            Id = saved.Id,
            StudentUserId = saved.StudentUserId,
            StudentEmail = student.Email,
            StudentFullName = profile?.FullName ?? student.Email,
            RollNo = profile?.MemberNumber ?? 0,
            ClassRoomId = saved.ClassRoomId,
            ClassRoomName = string.Empty,   // caller doesn't need it on success
            EnrolledAt = saved.EnrolledAt,
        };
    }

    public async Task<List<EnrollmentDto>> GetByClassRoomAsync(Guid classRoomId)
    {
        var enrollments = await _repository.GetByClassRoomIdAsync(classRoomId);
        var userIds = enrollments.Select(e => e.StudentUserId).ToList();

        var profiles = await _profileRepository.GetByUserIdsAsync(userIds);
        var profileByUserId = profiles.ToDictionary(p => p.UserId);

        return enrollments.Select(e =>
        {
            profileByUserId.TryGetValue(e.StudentUserId, out var profile);
            var email = e.StudentUser.Email;
            var name = !string.IsNullOrWhiteSpace(profile?.FullName) ? profile!.FullName : email;

            return new EnrollmentDto
            {
                Id = e.Id,
                StudentUserId = e.StudentUserId,
                StudentEmail = email,
                StudentFullName = name,
                RollNo = profile?.MemberNumber ?? 0,
                ClassRoomId = e.ClassRoomId,
                ClassRoomName = e.ClassRoom?.Name ?? string.Empty,
                EnrolledAt = e.EnrolledAt,
            };
        }).ToList();
    }

    public async Task EnsureEnrolledAsync(Guid studentUserId, Guid classRoomId)
    {
        if (await _repository.ExistsAsync(studentUserId, classRoomId))
            return;   // already here — no-op

        await EnsureStudentNotInAnotherClassAsync(studentUserId, classRoomId);

        await _repository.AddAsync(new Enrollment
        {
            StudentUserId = studentUserId,
            ClassRoomId = classRoomId,
        });
    }

    public async Task<bool> RemoveAsync(Guid studentUserId, Guid classRoomId)
        => await _repository.RemoveAsync(studentUserId, classRoomId);

    public async Task<EnrollmentDto?> GetMyEnrollmentAsync(Guid studentUserId)
    {
        var e = await _repository.GetByStudentUserIdAsync(studentUserId);
        if (e is null) return null;

        var profile = await _profileRepository.GetByUserIdAsync(studentUserId);

        return new EnrollmentDto
        {
            Id = e.Id,
            StudentUserId = e.StudentUserId,
            StudentEmail = e.StudentUser.Email,
            StudentFullName = profile?.FullName ?? e.StudentUser.Email,
            RollNo = profile?.MemberNumber ?? 0,
            ClassRoomId = e.ClassRoomId,
            ClassRoomName = e.ClassRoom.Name,
            EnrolledAt = e.EnrolledAt,
        };
    }
}