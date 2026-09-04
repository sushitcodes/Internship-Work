using Form.DTOs;
using Form.Entities;
using Form.Interfaces;
namespace Form.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _repository;
    private readonly IUserRepository _userRepository;

    public EnrollmentService(IEnrollmentRepository repository, IUserRepository userRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
    }

    public async Task<EnrollmentDto> EnrollAsync(CreateEnrollmentRequest request)
    {
        if (await _repository.ExistsAsync(request.StudentUserId, request.ClassRoomId))
            throw new InvalidOperationException("This student is already enrolled in this class.");

        var student = await _userRepository.GetByIdAsync(request.StudentUserId);
        if (student is null)
            throw new InvalidOperationException("Student not found.");

        var enrollment = new Enrollment
        {
            StudentUserId = request.StudentUserId,
            ClassRoomId = request.ClassRoomId,
        };
        var saved = await _repository.AddAsync(enrollment);

        return new EnrollmentDto
        {
            Id = saved.Id,
            StudentUserId = saved.StudentUserId,
            StudentEmail = student.Email,
            ClassRoomId = saved.ClassRoomId,
            EnrolledAt = saved.EnrolledAt,
        };
    }

    public async Task<List<EnrollmentDto>> GetByClassRoomAsync(Guid classRoomId) =>
        (await _repository.GetByClassRoomIdAsync(classRoomId)).Select(e => new EnrollmentDto
        {
            Id = e.Id,
            StudentUserId = e.StudentUserId,
            StudentEmail = e.StudentUser.Email,
            ClassRoomId = e.ClassRoomId,
            EnrolledAt = e.EnrolledAt,
        }).ToList();
    // Idempotent version of EnrollAsync — used by the submission flow, where
    // "already enrolled" is a normal, expected outcome, not an error. The
    // explicit "Enroll" button on ClassRoomsPage keeps using EnrollAsync
    // (which correctly throws), since a Staff member deliberately re-enrolling
    // someone who's already in the class IS worth flagging as a mistake.
    public async Task EnsureEnrolledAsync(Guid studentUserId, Guid classRoomId)
    {
        if (await _repository.ExistsAsync(studentUserId, classRoomId))
            return; // already enrolled — nothing to do, not an error

        await _repository.AddAsync(new Enrollment
        {
            StudentUserId = studentUserId,
            ClassRoomId = classRoomId,
        });
    }
}