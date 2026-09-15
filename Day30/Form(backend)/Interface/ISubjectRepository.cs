using Form.Entities;
namespace Form.Interfaces;

public interface ISubjectRepository
{
    Task<List<Subject>> GetByClassRoomIdAsync(Guid classRoomId);
    Task AddAsync(Subject subject);
    Task DeleteAsync(Guid id);
}