using Form.DTOs;
using Form.Entities;
using Form.Interfaces;
namespace Form.Services;

public class SubjectService(ISubjectRepository repository) : ISubjectService
{

    public async Task<List<SubjectDto>> GetByClassRoomAsync(Guid classRoomId) =>
        (await repository.GetByClassRoomIdAsync(classRoomId))
            .Select(s => new SubjectDto { Id = s.Id, ClassRoomId = s.ClassRoomId, Name = s.Name })
            .ToList();

    public async Task<SubjectDto> CreateAsync(CreateSubjectRequest request)
    {
        var subject = new Subject { ClassRoomId = request.ClassRoomId, Name = request.Name };
        await repository.AddAsync(subject);
        return new SubjectDto { Id = subject.Id, ClassRoomId = subject.ClassRoomId, Name = subject.Name };
    }

    public async Task DeleteAsync(Guid id) => await repository.DeleteAsync(id);
}