using Form.DTOs;
namespace Form.Interfaces;

public interface IGradeRepository
{
    Task<List<GradeRosterEntryDto>> GetRosterAsync(Guid subjectId);
    Task UpsertRangeAsync(
        Guid subjectId,
        List<(Guid EnrollmentId, decimal MarksObtained, decimal MaxMarks, string? Remarks)> entries,
        Guid gradedByUserId);
    Task<StudentReportCardDto?> GetReportCardAsync(Guid studentUserId, Guid classRoomId);
}