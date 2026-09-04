using Form.DTOs;
using Form.Entities;
using Form.Interfaces;
namespace Form.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IAttendanceRepository _repository;
    public AttendanceService(IAttendanceRepository repository) => _repository = repository;

    public async Task<List<AttendanceRosterEntryDto>> GetRosterAsync(Guid classRoomId, DateOnly date) =>
        await _repository.GetRosterAsync(classRoomId, date);

    public async Task MarkAsync(MarkAttendanceRequest request, Guid markedByUserId)
    {
        // No more ExistsForDateAsync guard — marking today never blocks
        // marking (or re-marking) today again. That guard was the whole bug.
        var entries = request.Entries
            .Select(e => (e.EnrollmentId, Enum.Parse<AttendanceStatus>(e.Status)))
            .ToList();

        await _repository.UpsertRangeAsync(request.ClassRoomId, request.Date, entries, markedByUserId);
    }

    public async Task<List<AttendanceRecordDto>> GetForClassAsync(Guid classRoomId, DateOnly date) =>
        (await _repository.GetByClassRoomAndDateAsync(classRoomId, date)).Select(MapToDto).ToList();

    public async Task<List<AttendanceRecordDto>> GetForStudentAsync(Guid studentUserId) =>
        (await _repository.GetByStudentAsync(studentUserId)).Select(MapToDto).ToList();

    private static AttendanceRecordDto MapToDto(AttendanceRecord a) => new()
    {
        Id = a.Id,
        EnrollmentId = a.EnrollmentId,
        StudentUserId = a.Enrollment.StudentUserId,
        StudentEmail = a.Enrollment.StudentUser.Email,
        Date = a.Date,
        Status = a.Status.ToString(),
    };
}