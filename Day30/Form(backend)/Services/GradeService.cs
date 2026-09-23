using Form.DTOs;
using Form.Interface;
using Form.Interfaces;
namespace Form.Services;

public class GradeService(
    IGradeRepository repository,
    IUserProfileRepository profileRepository,
    INotificationService notificationService,
    IEnrollmentRepository enrollmentRepository) : IGradeService
{
    public async Task<List<GradeRosterEntryDto>> GetRosterAsync(Guid subjectId)
    {
        var roster = await repository.GetRosterAsync(subjectId);
        await AttachRealNamesAsync(roster);
        return roster;
    }

    public async Task SubmitAsync(SubmitGradesRequest request, Guid gradedByUserId)
    {
        var entries = request.Entries
            .Select(e => (e.EnrollmentId, e.MarksObtained, e.MaxMarks, e.Remarks))
            .ToList();

        // 1. Persist first. If the upsert fails, no notification goes out.
        await repository.UpsertRangeAsync(request.SubjectId, entries, gradedByUserId);

        // 2. Then notify each student whose grade was touched.
        await NotifyGradeTargetsAsync(entries, request.SubjectId);
    }

    private async Task NotifyGradeTargetsAsync(
        List<(Guid EnrollmentId, decimal MarksObtained, decimal MaxMarks, string? Remarks)> entries,
        Guid subjectId)
    {
        if (entries.Count == 0) return;

        var enrollmentIds = entries.Select(e => e.EnrollmentId).Distinct().ToList();

        // One DB round-trip for the whole batch — not one per student.
        var map = await enrollmentRepository
            .GetGradeNotificationMapAsync(enrollmentIds, subjectId);

        foreach (var entry in entries)
        {
            if (!map.TryGetValue(entry.EnrollmentId, out var target)) continue;

            await notificationService.NotifyGradePublishedAsync(
                studentUserId: target.StudentUserId,
                subjectName: target.SubjectName,
                marks: entry.MarksObtained,
                maxMarks: entry.MaxMarks);
        }
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

    // Same lazy-profile fallback as GetSheetAsync in AttendanceService — falls
    // back to email only for a student who never filled out their profile.
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