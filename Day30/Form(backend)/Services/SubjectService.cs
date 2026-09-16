using Form.DTOs;
using Form.Entities;
using Form.Exceptions;
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
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ValidateException("Subject name is Required");
        var existing = await repository.GetByNameIncludingArchivedAsync(
            request.ClassRoomId, request.Name.Trim());
        if (existing is not null)
        {
            if (!existing.IsArchived)
                throw new ConflictException(
                    $"A subject named '{request.Name}' already exists in this classroom.");

            // Reactivate the archived subject — no new row.
            existing.IsArchived = false;
            existing.ArchivedAt = null;
            await repository.SaveChangesAsync();

            return new SubjectDto
            {
                Id = existing.Id,
                ClassRoomId = existing.ClassRoomId,
                Name = existing.Name,
            };
        }

        var subject = new Subject
        {
            ClassRoomId = request.ClassRoomId,
            Name = request.Name.Trim(),
        };
        await repository.AddAsync(subject);
        await repository.SaveChangesAsync();

        return new SubjectDto
        {
            Id = subject.Id,
            ClassRoomId = subject.ClassRoomId,
            Name = subject.Name,
        };
    }

    public async Task ArchiveAsync(Guid id)
    {
        var subject = await repository.GetByIdAsync(id);
        if (subject is null)
            throw new NotFoundException("Subject not found.");

        subject.IsArchived = true;
        subject.ArchivedAt = DateTimeOffset.UtcNow;
        await repository.SaveChangesAsync();
    }
}