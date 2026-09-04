using Form.Entities;

namespace Form.Interfaces;

public interface ISubmissionRepository
{
    Task<Submission> AddAsync(Submission submission);
    Task<int> GetCountAsync();
    Task<(List<Submission>Items,int TotalCount)>GetPagedAsync(int page,int pageSize,string? search);
    Task<Submission?> GetByIdAsync(Guid id);
    Task<bool> DeleteAsync(Guid id);
    Task<Submission?> UpdateAsync(Guid id, Submission updated);
    Task<int> GetCountByUserAsync(Guid userId);
    Task<List<Submission>> GetRecentAsync(int count);
    Task<List<Submission>> GetRecentByUserAsync(Guid userId, int count);
}
