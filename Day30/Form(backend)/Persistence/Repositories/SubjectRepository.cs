using Form.Entities;
using Form.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace Form.Persistence.Repositories
{

    public class SubjectRepository(AppDbContext context) : ISubjectRepository
    {
        public async Task<List<Subject>> GetByClassRoomIdAsync(Guid classRoomId) =>
    await context.Subjects.AsNoTracking()
        .Where(s => s.ClassRoomId == classRoomId)
        .OrderBy(s => s.Name)
        .ToListAsync();

        public async Task AddAsync(Subject subject)
        {
            context.Subjects.Add(subject);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid id)
        {
            var subject = await context.Subjects.FindAsync(id);
            if (subject is null) return;
            context.Subjects.Remove(subject);
            await context.SaveChangesAsync();
        }
    }
}
