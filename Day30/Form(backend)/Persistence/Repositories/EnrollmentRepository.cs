using Form.Entities;
using Form.Interfaces;
using Form.Persistence;
using Microsoft.EntityFrameworkCore;
namespace Form.Repositories;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly AppDbContext _context;
    public EnrollmentRepository(AppDbContext context) => _context = context;

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
            .Where(e => e.ClassRoomId == classRoomId)
            .ToListAsync();
}