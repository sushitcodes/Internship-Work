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
}