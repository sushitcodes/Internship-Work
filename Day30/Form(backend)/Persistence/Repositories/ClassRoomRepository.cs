using Form.Entities;
using Form.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace Form.Persistence.Repositories;


public class ClassRoomRepository : IClassRoomRepository
{
    private readonly AppDbContext _context;
    public ClassRoomRepository(AppDbContext context) => _context = context;

    public async Task<ClassRoom> AddAsync(ClassRoom classRoom)
    {
        _context.ClassRooms.Add(classRoom);
        await _context.SaveChangesAsync();
        return classRoom;
    }

    public async Task<List<ClassRoom>> GetAllAsync() =>
        await _context.ClassRooms
            .Include(c => c.Enrollments)
            .OrderByDescending(c => c.AcademicYear)
            .ThenBy(c => c.Name)
            .ToListAsync();
}


