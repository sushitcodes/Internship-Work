using Form.Entities;
namespace Form.Interfaces;

public record ClassRoomRow(Guid Id, string Name, int AcademicYear, Guid? ClassTeacherUserId, string? ClassTeacherEmail, int StudentCount);

public interface IClassRoomRepository
{
    Task<ClassRoom> AddAsync(ClassRoom classRoom);
    Task<List<ClassRoom>> GetAllAsync();                 // kept so existing callers still compile
    Task<List<ClassRoomRow>> GetAllRowsAsync();          // lean projection for the Classes page

    Task<ClassRoom?> GetByNameAndYearIncludingDeletedAsync(string name, int academicYear);
    Task<ClassRoom?> GetByIdAsync(Guid id);              // active classes only
    Task<(int Total, int Inactive)> CountEnrollmentsAsync(Guid classRoomId);
    Task SaveChangesAsync();

    Task AssignClassTeacherAsync(Guid classRoomId, Guid? teacherUserId);
    Task<IReadOnlyList<Guid>> GetTeacherUserIdsAsync(Guid classRoomId, CancellationToken ct = default);
    Task<string?> GetNameAsync(Guid classRoomId, CancellationToken ct = default);
}