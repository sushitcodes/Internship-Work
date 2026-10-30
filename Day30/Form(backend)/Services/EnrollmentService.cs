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

    // The single source of truth for "one class at a time".
    // Uses the version that also sees inactive enrollments, because the
    // unique index on StudentUserId counts them.
    private async Task EnsureStudentNotInAnotherClassAsync(Guid studentUserId, Guid classRoomId)
    {
        var existing = await _repository.GetByStudentUserIdIncludingInactiveAsync(studentUserId);
        if (existing is not null && existing.ClassRoomId != classRoomId)
            throw new InvalidOperationException(
                $"Student is already enrolled in class '{existing.ClassRoom.Name}'. " +
                "Remove them from that class first.");
    }

    public async Task<EnrollmentDto> EnrollAsync(CreateEnrollmentRequest request)
    {
        // CHECK 1: Does the student exist?
        var student = await _userRepository.GetByIdAsync(request.StudentUserId)
            ?? throw new InvalidOperationException("Student not found.");

        // CHECK 2: Does the student have the "Student" role?
        if (!student.RoleAssignments.Any(r => r.Role == UserRole.Student))
            throw new InvalidOperationException("Only users with the Student role can be enrolled.");

        // CHECK 3: Is the student active?
        if (!student.IsActive)
            throw new InvalidOperationException("This account is deactivated. Reactivate it before enrolling.");

        // CHECK 4: Is the student already enrolled in THIS class?
        if (await _repository.ExistsAsync(request.StudentUserId, request.ClassRoomId))
            throw new InvalidOperationException("This student is already enrolled in this class.");

        // CHECK 5: Is the student enrolled in ANOTHER class?
        await EnsureStudentNotInAnotherClassAsync(request.StudentUserId, request.ClassRoomId);

        // ALL CHECKS PASSED — save!
        var saved = await _repository.AddAsync(new Enrollment
        {
            StudentUserId = request.StudentUserId,
            ClassRoomId = request.ClassRoomId,
        });

        var profile = await _profileRepository.GetByUserIdAsync(student.Id);
        // return to the dto after saving
        return new EnrollmentDto
        {
            Id = saved.Id,
            StudentUserId = saved.StudentUserId,
            StudentEmail = student.Email,
            StudentFullName = profile?.FullName ?? student.Email,
            RollNo = profile?.MemberNumber ?? 0,
            ClassRoomId = saved.ClassRoomId,
            ClassRoomName = string.Empty,
            EnrolledAt = saved.EnrolledAt,
        };
    }
    public async Task<List<EnrollmentDto>> GetByClassRoomAsync(Guid classRoomId)
    {
        // Step 1: Get enrollments (1 query)
        var enrollments = await _repository.GetByClassRoomIdAsync(classRoomId);

        // Step 2: Extract ALL student IDs into one list
        var userIds = enrollments.Select(e => e.StudentUserId).ToList();

        // Step 3: Fetch ALL profiles in ONE query
        var profiles = await _profileRepository.GetByUserIdsAsync(userIds);

        // Step 4: Build a Dictionary for fast lookup
        var profileByUserId = profiles.ToDictionary(p => p.UserId);

        // Step 5: Combine data in memory (no more DB calls)
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