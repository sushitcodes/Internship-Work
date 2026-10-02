using Form.DTOs;
using Form.Exceptions;
using Form.Interface;
using Form.Interfaces;
namespace Form.Services;

public class GradeService(
    IGradeRepository repository,
    IUserProfileRepository profileRepository,
    INotificationService notificationService,
    IEnrollmentRepository enrollmentRepository,
    ILogger<GradeService> logger) : IGradeService
{
    public async Task<List<GradeRosterEntryDto>> GetRosterAsync(Guid subjectId)
    {
        var roster = await repository.GetRosterAsync(subjectId);
        await AttachRealNamesAsync(roster);
        return roster;
    }
    public async Task SubmitAsync(SubmitGradesRequest request, Guid gradedByUserId)
    {
        if (request.Entries.Count == 0)
            throw new ValidateException("No grades were sent.");

        if (request.Entries.Select(e => e.EnrollmentId).Distinct().Count() != request.Entries.Count)
            throw new ValidateException("The same student appears more than once.");

        // ValidateException (not InvalidOperationException) so the global handler returns 400, not 500.
        foreach (var entry in request.Entries)
        {
            if (entry.MarksObtained < 0)
                throw new ValidateException($"Marks cannot be negative. Got: {entry.MarksObtained}");

            if (entry.MaxMarks <= 0)
                throw new ValidateException($"Max marks must be greater than 0. Got: {entry.MaxMarks}");

            if (entry.MarksObtained > entry.MaxMarks)
                throw new ValidateException(
                    $"Marks ({entry.MarksObtained}) cannot exceed max marks ({entry.MaxMarks})");
        }

        var entries = request.Entries
            .Select(e => (e.EnrollmentId, e.MarksObtained, e.MaxMarks, e.Remarks))
            .ToList();

        await repository.UpsertRangeAsync(request.SubjectId, entries, gradedByUserId);

        // Grades are saved. A notification problem must not turn that into an error for the teacher.
        try
        {
            await NotifyGradeTargetsAsync(entries, request.SubjectId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Grades saved for subject {SubjectId} but notifying students failed", request.SubjectId);
        }
    }

    private async Task NotifyGradeTargetsAsync(
        List<(Guid EnrollmentId, decimal MarksObtained, decimal MaxMarks, string? Remarks)> entries,
        Guid subjectId)
    {
        var enrollmentIds = entries.Select(e => e.EnrollmentId).Distinct().ToList();
        var map = await enrollmentRepository.GetGradeNotificationMapAsync(enrollmentIds, subjectId);

        var drafts = new List<NotificationDraft>();
        foreach (var entry in entries)
        {
            if (!map.TryGetValue(entry.EnrollmentId, out var target)) continue;

            var pct = entry.MaxMarks > 0 ? entry.MarksObtained / entry.MaxMarks * 100 : 0;
            drafts.Add(new NotificationDraft(
                target.StudentUserId,
                "New Grade Published",
                $"Your grade for {target.SubjectName} has been recorded: {entry.MarksObtained}/{entry.MaxMarks} ({pct:0.#}%).",
                "/grades/report-card",
                "grade"));
        }

        await notificationService.NotifyManyAsync(drafts);   // ONE save, parallel push
    }

    public async Task<StudentReportCardDto?> GetReportCardAsync(Guid studentUserId, Guid classRoomId)
    {
        var card = await repository.GetReportCardAsync(studentUserId, classRoomId);
        if (card is null) return null;

        var profiles = await profileRepository.GetByUserIdsAsync(new List<Guid> { studentUserId });
        var profile = profiles.FirstOrDefault();
        if (profile is not null) card.StudentName = profile.FullName;

        return card;
    }

    private async Task AttachRealNamesAsync(List<GradeRosterEntryDto> roster)
    {
        var userIds = roster.Select(r => r.StudentUserId).ToList();
        var profiles = await profileRepository.GetByUserIdsAsync(userIds);
        var nameByUserId = profiles.ToDictionary(p => p.UserId, p => p.FullName);

        foreach (var entry in roster)
            if (nameByUserId.TryGetValue(entry.StudentUserId, out var name))
                entry.StudentName = name;
    }
}