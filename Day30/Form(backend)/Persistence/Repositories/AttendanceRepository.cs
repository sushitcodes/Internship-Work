using DocumentFormat.OpenXml.Wordprocessing;
using Form.DTOs;
using Form.Entities;
using Form.Interfaces;
using Form.Persistence;
using Microsoft.EntityFrameworkCore;
namespace Form.Repositories;

public class AttendanceRepository(AppDbContext Context) : IAttendanceRepository
{
    public async Task<List<AttendanceRosterEntryDto>> GetRosterAsync(Guid classRoomId, DateOnly date)
    {
        var todaysAttendance = Context.AttendanceRecords.AsNoTracking().Where(a => a.Date == date);

        return await (
            from e in Context.Enrollments.AsNoTracking()
            where e.ClassRoomId == classRoomId
            //  left join UserProfile so we can show FullName + MemberNumber.
            // EF translates this to a LEFT JOIN, so an enrollment whose profile
            // has not been created yet still appears in the roster (with a
            // fallback to email + roll number 0) instead of vanishing.
            join p in Context.UserProfiles.AsNoTracking()
                on e.StudentUserId equals p.UserId into profileJoined
            from p in profileJoined.DefaultIfEmpty()
            join a in todaysAttendance on e.Id equals a.EnrollmentId into joined
            from a in joined.DefaultIfEmpty()
            select new AttendanceRosterEntryDto
            {
                EnrollmentId = e.Id,
                StudentUserId = e.StudentUserId,
                StudentEmail = e.StudentUser.Email,
                //  fall back to the email prefix when the profile is missing,
                // matching what UserService.CreateUserAsync does for FullName.
                StudentName = p != null
                    ? p.FullName
                    : e.StudentUser.Email.Substring(0, e.StudentUser.Email.IndexOf('@')),
                //: MemberNumber is the identity roll number. 0 means "profile
                // missing" — the page can hide or label it if you want.
                RollNo = p != null ? p.MemberNumber : 0,
                AttendanceRecordId = a == null ? (Guid?)null : a.Id,
                Status = a == null ? "Unmarked" : a.Status.ToString(),
            }
        )
         //smallest roll number first; name as a tiebreaker for the
    // "profile missing" case where RollNo falls back to 0.
    .OrderBy(r => r.RollNo)
    .ThenBy(r => r.StudentName)
     .ToListAsync();
    }

    public async Task UpsertRangeAsync(
        Guid classRoomId, DateOnly date,
        List<(Guid EnrollmentId, AttendanceStatus Status)> entries,
        Guid markedByUserId)
    {
        var enrollmentIds = entries.Select(e => e.EnrollmentId).Distinct().ToList();

        // SECURITY: every enrollment in the request must belong to the class the caller
        // was authorised for. The controller only authorised classRoomId, not these ids.
        var validCount = await Context.Enrollments
            .CountAsync(e => e.ClassRoomId == classRoomId && enrollmentIds.Contains(e.Id));
        if (validCount != enrollmentIds.Count)
            throw new InvalidOperationException("One or more students do not belong to this class.");

        var existing = await Context.AttendanceRecords
            .Where(a => enrollmentIds.Contains(a.EnrollmentId) && a.Date == date)
            .ToDictionaryAsync(a => a.EnrollmentId);

        foreach (var (enrollmentId, status) in entries)
        {
            if (existing.TryGetValue(enrollmentId, out var record))
            {
                record.Status = status;
                record.MarkedByUserId = markedByUserId;
            }
            else
            {
                Context.AttendanceRecords.Add(new AttendanceRecord
                {
                    EnrollmentId = enrollmentId,
                    Date = date,
                    Status = status,
                    MarkedByUserId = markedByUserId,
                });
            }
        }

        try
        {
            await Context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            // Two teachers saved the same day at the same moment (unique index added in Part B).
            Context.ChangeTracker.Clear();
            throw new InvalidOperationException("Attendance was just saved by someone else. Reload and try again.");
        }
    }

    public async Task<List<AttendanceRecord>> GetByClassRoomAndDateAsync(Guid classRoomId, DateOnly date) =>
        await Context.AttendanceRecords
            .AsNoTracking()
            .Include(a => a.Enrollment).ThenInclude(e => e.StudentUser)
            .Where(a => a.Enrollment.ClassRoomId == classRoomId && a.Date == date)
            .ToListAsync();

    public async Task<List<AttendanceRecord>> GetByStudentAsync(Guid studentUserId) =>
        await Context.AttendanceRecords
            .AsNoTracking()
            .Include(a => a.Enrollment).ThenInclude(e => e.StudentUser)
            .Where(a => a.Enrollment.StudentUserId == studentUserId)
            .OrderByDescending(a => a.Date)
            .ToListAsync();

    // NEW (used by the dashboard, see item #18): counts in SQL instead of loading every row.
    public async Task<(int Present, int Total)> GetStudentCountsAsync(Guid studentUserId)
    {
        var q = Context.AttendanceRecords.AsNoTracking()
            .Where(a => a.Enrollment.StudentUserId == studentUserId);
        var total = await q.CountAsync();
        var present = await q.CountAsync(a => a.Status == AttendanceStatus.Present);
        return (present, total);
    }

    // NEW: only the most recent N rows.
    public async Task<List<AttendanceRecord>> GetRecentByStudentAsync(Guid studentUserId, int count) =>
        await Context.AttendanceRecords
            .AsNoTracking()
            .Include(a => a.Enrollment).ThenInclude(e => e.StudentUser)
            .Where(a => a.Enrollment.StudentUserId == studentUserId)
            .OrderByDescending(a => a.Date)
            .Take(count)
            .ToListAsync();

    public async Task<(int Present, int TotalMarked, List<(string Status, int Count)> Breakdown)> GetTodayStatsAsync(DateOnly date)
    {
        var breakdown = await Context.AttendanceRecords
            .AsNoTracking()
            .Where(a => a.Date == date)
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var typed = breakdown.Select(x => (x.Status.ToString(), x.Count)).ToList();
        var total = typed.Sum(x => x.Count);
        var present = typed.FirstOrDefault(x => x.Item1 == AttendanceStatus.Present.ToString()).Count;

        return (present, total, typed);
    }

    // The Include was removed: the Where clause joins on its own and nothing reads the navigation.
    public async Task<List<AttendanceRecord>> GetByClassRoomAndDateRangeAsync(Guid classRoomId, DateOnly start, DateOnly end) =>
        await Context.AttendanceRecords
            .AsNoTracking()
            .Where(a => a.Enrollment.ClassRoomId == classRoomId && a.Date >= start && a.Date <= end)
            .ToListAsync();
}