using Form.Entities;
using Form.DTOs;
namespace Form.Interfaces;

public interface IAttendanceRepository
{
    Task<List<AttendanceRosterEntryDto>> GetRosterAsync(Guid classRoomId, DateOnly date);
    Task UpsertRangeAsync(Guid classRoomId, DateOnly date, List<(Guid EnrollmentId, AttendanceStatus Status)> entries, Guid markedByUserId);
    Task<List<AttendanceRecord>> GetByClassRoomAndDateAsync(Guid classRoomId, DateOnly date);
    Task<List<AttendanceRecord>> GetByStudentAsync(Guid studentUserId);
    // Returns (present, totalMarked) for ONE date, across the whole school —
    // not per-class. "Present" here means status == Present specifically,
    // not "anything other than absent" — Late/Excused count toward totalMarked
    // but not toward present, which is the honest reading of the number.
    Task<(int Present, int TotalMarked)> GetTodaySummaryAsync(DateOnly date);
}