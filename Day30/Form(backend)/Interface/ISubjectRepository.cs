using Form.Entities;

namespace Form.Interfaces;

public interface ISubjectRepository
{
    Task<List<Subject>> GetByClassRoomIdAsync(Guid classRoomId);
    Task<Subject?> GetByNameIncludingArchivedAsync(Guid classRoomId, string name);
    Task<Subject?> GetByIdAsync(Guid id);
    Task AddAsync(Subject subject);
    Task SaveChangesAsync();
}