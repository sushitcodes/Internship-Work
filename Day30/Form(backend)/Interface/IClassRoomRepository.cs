using Form.Entities;
namespace Form.Interfaces;

public interface IClassRoomRepository
{
    Task<ClassRoom> AddAsync(ClassRoom classRoom);
    Task<List<ClassRoom>> GetAllAsync();
}