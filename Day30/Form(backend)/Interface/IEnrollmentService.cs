using Form.DTOs;
namespace Form.Interfaces;

public interface IEnrollmentService
{

    Task<EnrollmentDto> EnrollAsync(CreateEnrollmentRequest request);
 
    Task<List<EnrollmentDto>> GetByClassRoomAsync(Guid classRoomId);
}