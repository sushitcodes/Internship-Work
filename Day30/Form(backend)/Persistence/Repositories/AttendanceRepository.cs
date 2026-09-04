using Form.DTOs;
using Form.Entities;
using Form.Interfaces;
using Form.Persistence;
using Microsoft.EntityFrameworkCore;
namespace Form.Repositories;

public class AttendanceRepository : IAttendanceRepository
{
    private readonly AppDbContext _context;
    public AttendanceRepository(AppDbContext context) => _context = context;

    public async Task<List<AttendanceRosterEntryDto>> GetRosterAsync(Guid classRoomId, DateOnly date)
    {
        // Pre-filter attendance to just this date FIRST — this becomes the
        // right-hand side of the LEFT JOIN below, not a filter applied after.
        var todaysAttendance = _context.AttendanceRecords.Where(a => a.Date == date);

        // THE core fix: start from the roster (every enrolled student),
        // LEFT JOIN to attendance. A student with no matching row on the
        // right comes through with `a == null` — that's what makes
        // "Unmarked" a real, honest third state instead of a guess.
        return await (
            from e in _context.Enrollments
            where e.ClassRoomId == classRoomId
            join a in todaysAttendance on e.Id equals a.EnrollmentId into joined
            from a in joined.DefaultIfEmpty()
            select new AttendanceRosterEntryDto
            {
                EnrollmentId = e.Id,
                StudentUserId = e.StudentUserId,
                StudentEmail = e.StudentUser.Email,
                AttendanceRecordId = a == null ? (Guid?)null : a.Id,
                Status = a == null ? "Unmarked" : a.Status.ToString(),
            }
        ).ToListAsync();
    }

    public async Task UpsertRangeAsync(
        Guid classRoomId, DateOnly date,
        List<(Guid EnrollmentId, AttendanceStatus Status)> entries,
        Guid markedByUserId)
    {
        var enrollmentIds = entries.Select(e => e.EnrollmentId).ToList();

        // ONE query to find whichever of these students already have a
        // record today — not one exists-check per student, and no
        // all-or-nothing guard blocking the whole batch.
        var existing = await _context.AttendanceRecords
            .Where(a => enrollmentIds.Contains(a.EnrollmentId) && a.Date == date)
            .ToDictionaryAsync(a => a.EnrollmentId);

        foreach (var (enrollmentId, status) in entries)
        {
            if (existing.TryGetValue(enrollmentId, out var record))
            {
                // UPDATE path — this is what was missing entirely before.
                record.Status = status;
                record.MarkedByUserId = markedByUserId;
            }
            else
            {
                // CREATE path — only for students genuinely being marked
                // for the first time today.
                _context.AttendanceRecords.Add(new AttendanceRecord
                {
                    EnrollmentId = enrollmentId,
                    Date = date,
                    Status = status,
                    MarkedByUserId = markedByUserId,
                });
            }
        }

        await _context.SaveChangesAsync();
    }

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
    public async Task<(int Present, int TotalMarked)> GetTodaySummaryAsync(DateOnly date)
    {
        var todaysRecords = _context.AttendanceRecords.Where(a => a.Date == date);
        var total = await todaysRecords.CountAsync();
        var present = await todaysRecords.CountAsync(a => a.Status == AttendanceStatus.Present);
        return (present, total);
    }
}