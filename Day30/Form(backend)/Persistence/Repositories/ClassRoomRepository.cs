using Form.Entities;
using Form.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace Form.Persistence.Repositories;


public class ClassRoomRepository(AppDbContext _context) : IClassRoomRepository
{
    public async Task<ClassRoom> AddAsync(ClassRoom classRoom)
    {
        _context.ClassRooms.Add(classRoom);
        await _context.SaveChangesAsync();
        return classRoom;
    }

    public async Task<List<ClassRoom>> GetAllAsync() =>
        await _context.ClassRooms
        .AsNoTracking()
        
            .Include(c => c.Enrollments)
        .Include(c => c.ClassTeacher)
            .OrderByDescending(c => c.AcademicYear)
            .ThenBy(c => c.Name)
            .ToListAsync();

    public async Task AssignClassTeacherAsync(Guid classRoomId, Guid? teacherUserId)
    {
        var classRoom = await _context.ClassRooms.FirstOrDefaultAsync(c => c.Id == classRoomId);
        if (classRoom == null) throw new InvalidOperationException("Class not found.");
        classRoom.ClassTeacherUserId = teacherUserId;
        await _context.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Guid>> GetTeacherUserIdsAsync(
    Guid classRoomId, CancellationToken ct = default)
    {
        var teacherId = await _context.ClassRooms
            .Where(c => c.Id == classRoomId)
            .Select(c => c.ClassTeacherUserId)
            .FirstOrDefaultAsync(ct);

        return teacherId is null ? Array.Empty<Guid>() : new[] { teacherId.Value };
    }

    public async Task<string?> GetNameAsync(
        Guid classRoomId, CancellationToken ct = default)
        => await _context.ClassRooms
            .Where(c => c.Id == classRoomId)
            .Select(c => c.Name)
            .FirstOrDefaultAsync(ct);
}


