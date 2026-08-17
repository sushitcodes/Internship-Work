using Form.Entities;

namespace Form.Interfaces;

public interface ISubmissionRepository
{
    Task<Submission> AddAsync(Submission submission);
    Task<List<Submission>> GetAllAsync();
    Task<Submission?> GetByIdAsync(Guid id);
}
