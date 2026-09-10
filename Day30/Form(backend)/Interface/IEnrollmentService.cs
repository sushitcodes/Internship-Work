using Form.DTOs;
namespace Form.Interfaces;

public interface IEnrollmentService
{
    Task<EnrollmentDto> EnrollAsync(CreateEnrollmentRequest request);
    Task<List<EnrollmentDto>> GetByClassRoomAsync(Guid classRoomId);
    Task EnsureEnrolledAsync(Guid studentUserId, Guid classRoomId);
    Task<bool> RemoveAsync(Guid studentUserId, Guid classRoomId);  
    Task<EnrollmentDto?> GetMyEnrollmentAsync(Guid studentUserId);  
}