using Form.Entities;
using Form.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace Form.Persistence.Repositories
{

    public class SubjectRepository(AppDbContext context) : ISubjectRepository
    {
        // Normal read — archived are filtered out by the query filter.

        public async Task<List<Subject>> GetByClassRoomIdAsync(Guid classRoomId) =>
    await context.Subjects.AsNoTracking()
        .Where(s => s.ClassRoomId == classRoomId)
        .OrderBy(s => s.Name)
        .ToListAsync();
        // Must see archived rows (for reactivation) — bypass the query filter.
        public Task<Subject?> GetByNameIncludingArchivedAsync(Guid classRoomId, string name) => context.Subjects
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(s =>
            s.ClassRoomId == classRoomId &&
            s.Name == name);
        // Tracked — ArchiveAsync mutates this entity.
        // No AsNoTracking here, or the save would silently do nothing.
        public Task<Subject?> GetByIdAsync(Guid id) =>
            context.Subjects
                .FirstOrDefaultAsync(s => s.Id == id);

        // Just add — caller decides when to save.
        public async Task AddAsync(Subject subject)
        {
            context.Subjects.Add(subject);
            await context.SaveChangesAsync();

        }
        // The service calls this after making changes.

        public Task SaveChangesAsync() => context.SaveChangesAsync();
    }
    }
