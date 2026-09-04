using Form.DTOs;
namespace Form.Interfaces;

public interface IAttendanceService
{
    Task<List<AttendanceRosterEntryDto>> GetRosterAsync(Guid classRoomId, DateOnly date);
    Task MarkAsync(MarkAttendanceRequest request, Guid markedByUserId);
    Task<List<AttendanceRecordDto>> GetForClassAsync(Guid classRoomId, DateOnly date);
    Task<List<AttendanceRecordDto>> GetForStudentAsync(Guid studentUserId);
}