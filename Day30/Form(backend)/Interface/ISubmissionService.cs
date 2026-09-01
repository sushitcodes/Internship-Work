
using Form.DTOs;
using static Form.DTOs.ClassRoomDtos;

namespace Form.Interfaces;


public interface ISubmissionService
{
    Task<SubmissionDto> CreateSubmissionAsync(CreateSubmissionRequest request);
    Task<SubmissionDto?> GetByIdAsync(Guid id);
    Task<int> GetCountAsync();
    Task<bool> DeleteAsync(Guid id);
    Task<PagedResult<SubmissionDto>> GetPagedAsync(int page, int pageSize, string? search);
    Task<SubmissionDto?> UpdateAsync(Guid id, UpdateSubmissionRequest request);



}
