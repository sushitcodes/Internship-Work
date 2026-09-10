using Form.Entities;
namespace Form.Interfaces;

public interface IEnrollmentRepository
{
    Task<Enrollment> AddAsync(Enrollment enrollment);
    Task<bool> ExistsAsync(Guid studentUserId, Guid classRoomId);
    Task<List<Enrollment>> GetByClassRoomIdAsync(Guid classRoomId);
    Task<Enrollment?> GetByStudentUserIdAsync(Guid studentUserId);
    Task<bool>RemoveAsync(Guid studentUserId, Guid classRoomId);
}