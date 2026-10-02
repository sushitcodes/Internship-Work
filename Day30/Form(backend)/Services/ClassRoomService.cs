using Form.DTOs;
using Form.Entities;
using Form.Exceptions;
using Form.Interfaces;
namespace Form.Services;

public class ClassRoomService(IClassRoomRepository repository, IUserProfileRepository profileRepository, IUserRepository userRepository) : IClassRoomService
{
    public async Task<ClassRoomDto> CreateAsync(CreateClassRoomRequest request)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        if (name.Length is 0 or > 100)
            throw new ValidateException("Class name is required (max 100 characters).");
        if (request.AcademicYear is < 1900 or > 2200)
            throw new ValidateException("Academic year is not valid.");

        var existing = await repository.GetByNameAndYearIncludingDeletedAsync(name, request.AcademicYear);

        if (existing is not null)
        {
            // Active class with this name: not allowed.
            if (!existing.IsDeleted)
                throw new ConflictException($"A class named '{existing.Name}' already exists for {existing.AcademicYear}.");

            // Deleted class with this name: bring it back instead of inserting a duplicate.
            existing.IsDeleted = false;
            existing.DeletedAt = null;
            existing.ClassTeacherUserId = null;     // already cleared at delete time; the admin reassigns
            await repository.SaveChangesAsync();

            return new ClassRoomDto
            {
                Id = existing.Id,
                Name = existing.Name,
                AcademicYear = existing.AcademicYear,
                StudentCount = 0,
                WasRestored = true,
            };
        }

        var saved = await repository.AddAsync(new ClassRoom { Name = name, AcademicYear = request.AcademicYear });
        return new ClassRoomDto
        {
            Id = saved.Id,
            Name = saved.Name,
            AcademicYear = saved.AcademicYear,
            StudentCount = 0,
        };
    }

    public async Task DeleteAsync(Guid classRoomId)
    {
        var classRoom = await repository.GetByIdAsync(classRoomId)
            ?? throw new NotFoundException("Class not found.");

        var (total, inactive) = await repository.CountEnrollmentsAsync(classRoomId);
        if (total > 0)
        {
            var extra = inactive > 0
                ? $" ({inactive} of them are deactivated accounts: reactivate and remove them, or move them to another class)"
                : string.Empty;
            throw new ConflictException(
                $"This class still has {total} enrolled student(s){extra}. Remove or move them before deleting the class.");
        }

        classRoom.IsDeleted = true;
        classRoom.DeletedAt = DateTime.UtcNow;
        classRoom.ClassTeacherUserId = null;    // a hidden class must not keep teacher rights
        await repository.SaveChangesAsync();
    }

    public async Task<List<ClassRoomDto>> GetAllAsync()
    {
        var rows = await repository.GetAllRowsAsync();

        // One batch name lookup for every class teacher (no N+1).
        var teacherIds = rows.Where(c => c.ClassTeacherUserId.HasValue)
            .Select(c => c.ClassTeacherUserId!.Value).Distinct().ToList();
        var profiles = await profileRepository.GetByUserIdsAsync(teacherIds);
        var nameByUserId = profiles.ToDictionary(p => p.UserId, p => p.FullName);

        return rows.Select(c => new ClassRoomDto
        {
            Id = c.Id,
            Name = c.Name,
            AcademicYear = c.AcademicYear,
            StudentCount = c.StudentCount,
            ClassTeacherUserId = c.ClassTeacherUserId,
            ClassTeacherName = c.ClassTeacherUserId is null
                ? null
                : nameByUserId.TryGetValue(c.ClassTeacherUserId.Value, out var name) ? name : c.ClassTeacherEmail,
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