using Form.DTOs;
using Form.Entities;
using Form.Interface;
using Form.Interfaces;
namespace Form.Services;

public class ClassRoomService(IClassRoomRepository repository, IUserProfileRepository profileRepository, IUserRepository userRepository) : IClassRoomService
{
    public async Task<ClassRoomDto> CreateAsync(CreateClassRoomRequest request)
    {
        var classRoom = new ClassRoom { Name = request.Name, AcademicYear = request.AcademicYear };
        var saved = await repository.AddAsync(classRoom);
        return new ClassRoomDto
        {
            Id = saved.Id,
            Name = saved.Name,
            AcademicYear = saved.AcademicYear,
            StudentCount = 0,
        };
    }

    public async Task<List<ClassRoomDto>> GetAllAsync()
    {
        var classRooms = await repository.GetAllAsync();

        // One batch name lookup for every class teacher on the page — same
        // N+1 avoidance as the attendance sheet and the grade roster.
        var teacherIds = classRooms
            .Where(c => c.ClassTeacherUserId.HasValue)
            .Select(c => c.ClassTeacherUserId!.Value)
            .Distinct()
            .ToList();
        var profiles = await profileRepository.GetByUserIdsAsync(teacherIds);
        var nameByUserId = profiles.ToDictionary(p => p.UserId, p => p.FullName);

        return classRooms.Select(c => new ClassRoomDto
        {
            Id = c.Id,
            Name = c.Name,
            AcademicYear = c.AcademicYear,
            StudentCount = c.Enrollments.Count,
            ClassTeacherUserId = c.ClassTeacherUserId,
            ClassTeacherName = c.ClassTeacherUserId is null
                ? null
                : nameByUserId.TryGetValue(c.ClassTeacherUserId.Value, out var name)
                    ? name
                    : c.ClassTeacher?.Email, // lazy-profile fallback, same pattern as everywhere else
        }).ToList();
    }

    public async Task AssignClassTeacherAsync(Guid classRoomId, Guid? teacherUserId)
    {
        if (teacherUserId is null)
        {
            await repository.AssignClassTeacherAsync(classRoomId, null);
            return;
        }
        var teacher = await userRepository.GetByIdAsync(teacherUserId.Value);   

        if (teacher is null)                                                  
            throw new InvalidOperationException("User not found.");


        var isEligible = teacher.RoleAssignments.Any(ra => ra.Role == UserRole.Staff || ra.Role == UserRole.Admin);
        if (!isEligible)
            throw new InvalidOperationException("Only a Staff or Admin user can be appointed class teacher.");

        await repository.AssignClassTeacherAsync(classRoomId, teacherUserId);
    }
}