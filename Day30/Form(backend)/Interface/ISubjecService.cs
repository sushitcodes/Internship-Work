using Form.DTOs;
namespace Form.Interfaces;

public interface ISubjectService
{
    Task<List<SubjectDto>> GetByClassRoomAsync(Guid classRoomId);
    Task<SubjectDto> CreateAsync(CreateSubjectRequest request);
    Task ArchiveAsync(Guid id);
}