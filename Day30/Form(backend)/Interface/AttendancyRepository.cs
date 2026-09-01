using Form.Entities;
namespace Form.Interfaces;

public interface IAttendanceRepository
{
    Task AddRangeAsync(List<AttendanceRecord> records);
    Task<bool> ExistsForDateAsync(Guid classRoomId, DateOnly date);
    Task<List<AttendanceRecord>> GetByClassRoomAndDateAsync(Guid classRoomId, DateOnly date);
    Task<List<AttendanceRecord>> GetByStudentAsync(Guid studentUserId);
}