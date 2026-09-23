using Form.Entities;
namespace Form.Interfaces;

public interface IClassRoomRepository
{
    Task<ClassRoom> AddAsync(ClassRoom classRoom);
    Task<List<ClassRoom>> GetAllAsync();

    Task AssignClassTeacherAsync(Guid classRoomId, Guid? teacherUserId);
    Task<IReadOnlyList<Guid>> GetTeacherUserIdsAsync(Guid classRoomId, CancellationToken ct = default);
    Task<string?> GetNameAsync(Guid classRoomId, CancellationToken ct = default);

}