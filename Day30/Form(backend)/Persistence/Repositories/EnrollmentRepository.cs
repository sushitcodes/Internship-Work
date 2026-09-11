using Form.Entities;
using Form.Interfaces;
using Form.Persistence;
using Microsoft.EntityFrameworkCore;
namespace Form.Repositories;

public class EnrollmentRepository(AppDbContext _context) : IEnrollmentRepository
{
    public async Task<Enrollment> AddAsync(Enrollment enrollment)
    {
        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();
        return enrollment;
    }

    public async Task<bool> ExistsAsync(Guid studentUserId, Guid classRoomId) =>
        await _context.Enrollments.AnyAsync(e =>
            e.StudentUserId == studentUserId && e.ClassRoomId == classRoomId);

    public async Task<List<Enrollment>> GetByClassRoomIdAsync(Guid classRoomId) =>
        await _context.Enrollments
            .Include(e => e.StudentUser)
        .Include(e=> e.ClassRoom)
            .Where(e => e.ClassRoomId == classRoomId)
            .ToListAsync();

    

public async Task<Enrollment?> GetByStudentUserIdAsync(Guid studentUserId) =>
        await _context.Enrollments
        .Include(e => e.ClassRoom)
        .Include(e=> e.StudentUser)
        .FirstOrDefaultAsync(e => e.StudentUserId == studentUserId);
    public async Task<bool> RemoveAsync(Guid studentUserId, Guid classRoomId)
    {
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.StudentUserId == studentUserId && e.ClassRoomId == classRoomId);
        if (enrollment is null) return false;

        _context.Enrollments.Remove(enrollment);
        await _context.SaveChangesAsync();
        return true;
    }

}
