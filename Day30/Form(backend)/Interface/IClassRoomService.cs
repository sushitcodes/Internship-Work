using Form.DTOs;
namespace Form.Interfaces;

public interface IClassRoomService
{
    Task<ClassRoomDto> CreateAsync(CreateClassRoomRequest request);
    Task<List<ClassRoomDto>> GetAllAsync();
    Task AssignClassTeacherAsync(Guid classRoomId, Guid? teacherUserId);
}