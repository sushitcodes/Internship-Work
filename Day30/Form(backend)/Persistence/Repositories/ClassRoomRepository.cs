using Form.Entities;
using Form.Exceptions;
using Form.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace Form.Persistence.Repositories;

public class ClassRoomRepository(AppDbContext _context) : IClassRoomRepository
{
    public async Task<ClassRoom> AddAsync(ClassRoom classRoom)
    {
        _context.ClassRooms.Add(classRoom);
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            // Two admins created the same class at the same moment. The index decides who wins.
            _context.ChangeTracker.Clear();
            throw new ConflictException("A class with this name and academic year already exists.");
        }
        return classRoom;
    }

    // Kept for existing callers (GradesController). Hides deleted classes.
    public async Task<List<ClassRoom>> GetAllAsync() =>
        await _context.ClassRooms
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .Include(c => c.Enrollments)
            .Include(c => c.ClassTeacher)
            .OrderByDescending(c => c.AcademicYear)
            .ThenBy(c => c.Name)
            .ToListAsync();

    // One SQL query with exactly the columns the screen shows.
    // c.Enrollments.Count only counts ACTIVE enrollments (the Enrollment query filter applies).
    public async Task<List<ClassRoomRow>> GetAllRowsAsync() =>
        await _context.ClassRooms
            .AsNoTracking()
            .Where(c => !c.IsDeleted)
            .OrderByDescending(c => c.AcademicYear)
            .ThenBy(c => c.Name)
            .Select(c => new ClassRoomRow(
                c.Id,
                c.Name,
                c.AcademicYear,
                c.ClassTeacherUserId,
                c.ClassTeacher != null ? c.ClassTeacher.Email : null,
                c.Enrollments.Count))
            .ToListAsync();

    // Sees deleted rows too, because the unique index does.
    // Tracked (no AsNoTracking): the service may restore it and save.
    public async Task<ClassRoom?> GetByNameAndYearIncludingDeletedAsync(string name, int academicYear) =>
        await _context.ClassRooms
            .FirstOrDefaultAsync(c => c.Name == name && c.AcademicYear == academicYear);

    public async Task<ClassRoom?> GetByIdAsync(Guid id) =>
        await _context.ClassRooms.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

    // IgnoreQueryFilters: a deactivated student's enrollment is hidden by the Enrollment filter,
    // but it still counts. Otherwise reactivating them later would put them in a deleted class.
    public async Task<(int Total, int Inactive)> CountEnrollmentsAsync(Guid classRoomId)
    {
        var q = _context.Enrollments.IgnoreQueryFilters().Where(e => e.ClassRoomId == classRoomId);
        var total = await q.CountAsync();
        var inactive = await q.CountAsync(e => !e.IsActive);
        return (total, inactive);
    }

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();

    public async Task AssignClassTeacherAsync(Guid classRoomId, Guid? teacherUserId)
    {
        var classRoom = await _context.ClassRooms.FirstOrDefaultAsync(c => c.Id == classRoomId && !c.IsDeleted);
        if (classRoom == null) throw new InvalidOperationException("Class not found.");
        classRoom.ClassTeacherUserId = teacherUserId;
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Guid>> GetTeacherUserIdsAsync(
        Guid classRoomId, CancellationToken ct = default)
    {
        var teacherId = await _context.ClassRooms
            .Where(c => c.Id == classRoomId && !c.IsDeleted)
            .Select(c => c.ClassTeacherUserId)
            .FirstOrDefaultAsync(ct);

        return teacherId is null ? Array.Empty<Guid>() : new[] { teacherId.Value };
    }

    // A deleted class returns null, so "submit to this class" fails with "class not found".
    public async Task<string?> GetNameAsync(Guid classRoomId, CancellationToken ct = default)
        => await _context.ClassRooms
            .Where(c => c.Id == classRoomId && !c.IsDeleted)
            .Select(c => c.Name)
            .FirstOrDefaultAsync(ct);
}