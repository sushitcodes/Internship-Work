using Form.Entities;
using Form.Interfaces;
using Form.Persistence;
using Microsoft.EntityFrameworkCore;
namespace Form.Repositories;

public class AttendanceRepository : IAttendanceRepository
{
    private readonly AppDbContext _context;
    public AttendanceRepository(AppDbContext context) => _context = context;

    public async Task AddRangeAsync(List<AttendanceRecord> records)
    {
        _context.AttendanceRecords.AddRange(records);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsForDateAsync(Guid classRoomId, DateOnly date) =>
        await _context.AttendanceRecords
            .Include(a => a.Enrollment)
            .AnyAsync(a => a.Enrollment.ClassRoomId == classRoomId && a.Date == date);

    public async Task<List<AttendanceRecord>> GetByClassRoomAndDateAsync(Guid classRoomId, DateOnly date) =>
        await _context.AttendanceRecords
            .Include(a => a.Enrollment).ThenInclude(e => e.StudentUser)
            .Where(a => a.Enrollment.ClassRoomId == classRoomId && a.Date == date)
            .ToListAsync();

    public async Task<List<AttendanceRecord>> GetByStudentAsync(Guid studentUserId) =>
        await _context.AttendanceRecords
            .Include(a => a.Enrollment).ThenInclude(e => e.StudentUser)
            .Where(a => a.Enrollment.StudentUserId == studentUserId)
            .OrderByDescending(a => a.Date)
            .ToListAsync();
}