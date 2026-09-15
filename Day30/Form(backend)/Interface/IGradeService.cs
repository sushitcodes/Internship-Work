using Form.DTOs;
namespace Form.Interfaces;

public interface IGradeService
{
    Task<List<GradeRosterEntryDto>> GetRosterAsync(Guid subjectId);
    Task SubmitAsync(SubmitGradesRequest request, Guid gradedByUserId);
    Task<StudentReportCardDto?> GetReportCardAsync(Guid studentUserId, Guid classRoomId);
}