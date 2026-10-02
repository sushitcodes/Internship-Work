
using Form.DTOs;
namespace Form.Interfaces;


public interface ISubmissionService
{
    Task<SubmissionDto> CreateSubmissionAsync(CreateSubmissionRequest request);
    Task<SubmissionDto?> GetByIdAsync(Guid id);
    Task<int> GetCountAsync(Guid? createdByUserId = null);
    Task<bool> DeleteAsync(Guid id);
    Task<PagedResult<SubmissionDto>> GetPagedAsync(int page, int pageSize, string? search, Guid? createdByUserId = null);
    Task<SubmissionDto?> UpdateAsync(Guid id, UpdateSubmissionRequest request);
}
