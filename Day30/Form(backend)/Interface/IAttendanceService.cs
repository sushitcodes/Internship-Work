using Form.DTOs;
namespace Form.Interfaces;

public interface IAttendanceService
{
    Task<List<AttendanceRosterEntryDto>> GetRosterAsync(Guid classRoomId, DateOnly date);
    Task MarkAsync(MarkAttendanceRequest request, Guid markedByUserId);
    Task<List<AttendanceRecordDto>> GetForClassAsync(Guid classRoomId, DateOnly date);
    Task<List<AttendanceRecordDto>> GetForStudentAsync(Guid studentUserId); Task<AttendanceSheetDto> GetSheetAsync(Guid classRoomId, DateOnly start, DateOnly end);
    Task<byte[]> ExportSheetAsync(Guid classRoomId, DateOnly start, DateOnly end);
}