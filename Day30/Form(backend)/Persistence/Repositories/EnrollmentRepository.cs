using Form.Entities;
using Form.Interfaces;
using Form.Persistence;
using Microsoft.EntityFrameworkCore;
namespace Form.Repositories;

public class EnrollmentRepository(AppDbContext _context) : IEnrollmentRepository
{
    public async Task<Enrollment> AddAsync(Enrollment enrollment)
    {
        if (!await _context.ClassRooms.AnyAsync(c => c.Id == enrollment.ClassRoomId && !c.IsDeleted))
            throw new InvalidOperationException("Class not found.");

        _context.Enrollments.Add(enrollment);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            _context.ChangeTracker.Clear();   // drop the failed insert
            throw new InvalidOperationException("This student is already enrolled in a class.");
        }
        catch (DbUpdateException ex) when (ex.IsForeignKeyViolation())
        {
            _context.ChangeTracker.Clear();
            throw new InvalidOperationException("Class or student was not found.");
        }
        return enrollment;
    }

    // IgnoreQueryFilters: an INACTIVE enrollment still occupies the unique index,
    // so the "already enrolled?" check must see it too.
    public async Task<bool> ExistsAsync(Guid studentUserId, Guid classRoomId) =>
        await _context.Enrollments
            .IgnoreQueryFilters()
            .AnyAsync(e => e.StudentUserId == studentUserId && e.ClassRoomId == classRoomId);

    public async Task<List<Enrollment>> GetByClassRoomIdAsync(Guid classRoomId) =>
        await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.StudentUser)
            .Include(e => e.ClassRoom)
            .Where(e => e.ClassRoomId == classRoomId)
            .ToListAsync();

    public async Task<Enrollment?> GetByStudentUserIdAsync(Guid studentUserId) =>
        await _context.Enrollments
            .AsNoTracking()
            .Include(e => e.ClassRoom)
            .Include(e => e.StudentUser)
            .FirstOrDefaultAsync(e => e.StudentUserId == studentUserId);

    // NEW: same as above but also finds inactive rows. Used only by the
    // "one class per student" guard in EnrollmentService.
    public async Task<Enrollment?> GetByStudentUserIdIncludingInactiveAsync(Guid studentUserId) =>
        await _context.Enrollments
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Include(e => e.ClassRoom)
            .FirstOrDefaultAsync(e => e.StudentUserId == studentUserId);

    public async Task<bool> RemoveAsync(Guid studentUserId, Guid classRoomId)
    {
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.StudentUserId == studentUserId && e.ClassRoomId == classRoomId);
        if (enrollment is null) return false;

        _context.Enrollments.Remove(enrollment);   // see item #21: this also cascades to attendance + grades
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Dictionary<Guid, (Guid StudentUserId, string SubjectName)>>
    GetGradeNotificationMapAsync(
        IEnumerable<Guid> enrollmentIds,
        Guid subjectId,
        CancellationToken ct = default)
    {
        var ids = enrollmentIds.Distinct().ToList();

        var subjectName = await _context.Subjects
            .IgnoreQueryFilters()
            .Where(s => s.Id == subjectId)
            .Select(s => s.Name)
            .FirstOrDefaultAsync(ct) ?? "your subject";

        var rows = await _context.Enrollments
            .AsNoTracking()
            .Where(e => ids.Contains(e.Id))
            .Select(e => new { e.Id, e.StudentUserId })
            .ToListAsync(ct);

        return rows.ToDictionary(r => r.Id, r => (r.StudentUserId, subjectName));
    }

    // One UPDATE statement instead of "load every row, loop, save".
    public async Task DeactivateByStudentUserIdAsync(Guid studentUserId)
    {
        var now = DateTime.UtcNow;
        await _context.Enrollments
            .IgnoreQueryFilters()
            .Where(e => e.StudentUserId == studentUserId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(e => e.IsActive, false)
                .SetProperty(e => e.DeactivatedAt, now));
    }

    public async Task ReactivateByStudentUserIdAsync(Guid studentUserId)
    {
        await _context.Enrollments
            .IgnoreQueryFilters()
            .Where(e => e.StudentUserId == studentUserId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(e => e.IsActive, true)
                .SetProperty(e => e.DeactivatedAt, (DateTime?)null));
    }
}