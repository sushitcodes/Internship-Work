using Form.DTOs;
using static Form.Dtos.Class;
namespace Form.Interfaces;

public interface ISubmissionService
{
    Task<SubmissionDto> CreateSubmissionAsync(CreateSubmissionRequest request);
    Task<List<SubmissionDto>> GetAllAsync();
    Task<SubmissionDto?> GetByIdAsync(Guid id);
    Task<bool> DeleteAsync(Guid id);
}
