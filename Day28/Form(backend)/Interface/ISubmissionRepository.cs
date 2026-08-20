using Form.Entities;

namespace Form.Interfaces;

public interface ISubmissionRepository
{
    Task<Submission> AddAsync(Submission submission);
    Task<(List<Submission>Items,int TotalCount)>GetPagedAsync(int page,int pageSize,string? search);
    Task<Submission?> GetByIdAsync(Guid id);
    Task<bool> DeleteAsync(Guid id);
    Task<Submission?> UpdateAsync(Guid id, Submission updated);
}
