using Form.DTOs;
using Form.Entities;
using Form.Interfaces;
namespace Form.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IAttendanceRepository _repository;

    public AttendanceService(IAttendanceRepository repository)
    {
        _repository = repository;
    }

    public async Task MarkAsync(MarkAttendanceRequest request, Guid markedByUserId)
    {
        if (await _repository.ExistsForDateAsync(request.ClassRoomId, request.Date))
            throw new InvalidOperationException("Attendance for this class and date has already been recorded.");

        var records = request.Entries.Select(entry => new AttendanceRecord
        {
            EnrollmentId = entry.EnrollmentId,
            Date = request.Date,
            Status = Enum.Parse<AttendanceStatus>(entry.Status),
            MarkedByUserId = markedByUserId,
        }).ToList();

        await _repository.AddRangeAsync(records);
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