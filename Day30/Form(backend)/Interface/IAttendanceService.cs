using Form.DTOs;
namespace Form.Interfaces;

public interface IAttendanceService
{
    Task MarkAsync(MarkAttendanceRequest request, Guid markedByUserId);
    Task<List<AttendanceRecordDto>> GetForClassAsync(Guid classRoomId, DateOnly date);
    Task<List<AttendanceRecordDto>> GetForStudentAsync(Guid studentUserId);
}