using Form.Entities;

namespace Form.Interfaces;

public interface ISubmissionRepository
{
    Task<Submission> AddAsync(Submission submission);
    Task<int> GetCountAsync(Guid? createdByUserId = null);
    Task<(List<Submission> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, string? search, Guid? createdByUserId = null);
    Task<Submission?> GetByIdAsync(Guid id);
    Task<bool> DeleteAsync(Guid id);
    Task<Submission?> UpdateAsync(Guid id, Submission updated);
    Task<int> GetCountByUserAsync(Guid userId);
    Task<List<Submission>> GetRecentAsync(int count);
    Task<List<Submission>> GetRecentByUserAsync(Guid userId, int count);
}
