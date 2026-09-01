using Form.DTOs;
using Form.Entities;
using Form.Interfaces;
namespace Form.Services;

public class ClassRoomService : IClassRoomService
{
    private readonly IClassRoomRepository _repository;
    public ClassRoomService(IClassRoomRepository repository) => _repository = repository;

    public async Task<ClassRoomDto> CreateAsync(CreateClassRoomRequest request)
    {
        var classRoom = new ClassRoom { Name = request.Name, AcademicYear = request.AcademicYear };
        var saved = await _repository.AddAsync(classRoom);
        return MapToDto(saved);
    }

    public async Task<List<ClassRoomDto>> GetAllAsync() =>
        (await _repository.GetAllAsync()).Select(MapToDto).ToList();

    private static ClassRoomDto MapToDto(ClassRoom c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        AcademicYear = c.AcademicYear,
        StudentCount = c.Enrollments.Count,
    };
}