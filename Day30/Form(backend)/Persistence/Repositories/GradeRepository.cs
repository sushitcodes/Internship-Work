using Form.DTOs;
using Form.Entities;
using Form.Interfaces;
using Form.Persistence;
using Microsoft.EntityFrameworkCore;
namespace Form.Repositories;

public class GradeRepository(AppDbContext context) : IGradeRepository
{
    public async Task<List<GradeRosterEntryDto>> GetRosterAsync(Guid subjectId)
    {
        var subject = await context.Subjects.AsNoTracking().FirstOrDefaultAsync(s => s.Id == subjectId);
        if (subject is null) return new List<GradeRosterEntryDto>();

        var subjectGrades = context.Grades.AsNoTracking().Where(g => g.SubjectId == subjectId);

        // Exact same left-join-the-roster shape as AttendanceRepository.GetRosterAsync:
        // start from everyone enrolled in this subject's classroom, LEFT JOIN to
        // whatever grades already exist for this subject. Ungraded students come
        // through with GradeId == null instead of silently not appearing at all.
        return await (
            from e in context.Enrollments.AsNoTracking()
            where e.ClassRoomId == subject.ClassRoomId
            join g in subjectGrades on e.Id equals g.EnrollmentId into joined
            from g in joined.DefaultIfEmpty()
            select new GradeRosterEntryDto
            {
                EnrollmentId = e.Id,
                StudentUserId = e.StudentUserId,
                StudentName = e.StudentUser.Email, // real name filled in by GradeService
                GradeId = g == null ? (Guid?)null : g.Id,
                MarksObtained = g == null ? (decimal?)null : g.MarksObtained,
                MaxMarks = g == null ? 100 : g.MaxMarks,
                Remarks = g == null ? null : g.Remarks,
            }
        ).ToListAsync();
    }

    public async Task UpsertRangeAsync(
        Guid subjectId,
        List<(Guid EnrollmentId, decimal MarksObtained, decimal MaxMarks, string? Remarks)> entries,
        Guid gradedByUserId)
    {
        var enrollmentIds = entries.Select(e => e.EnrollmentId).ToList();

        // Same batch-lookup as attendance's UpsertRangeAsync — one query for
        // every existing grade in this batch, not one exists-check per student.
        var existing = await context.Grades
            .Where(g => enrollmentIds.Contains(g.EnrollmentId) && g.SubjectId == subjectId)
            .ToDictionaryAsync(g => g.EnrollmentId);

        foreach (var (enrollmentId, marks, maxMarks, remarks) in entries)
        {
            if (existing.TryGetValue(enrollmentId, out var grade))
            {
                grade.MarksObtained = marks;
                grade.MaxMarks = maxMarks;
                grade.Remarks = remarks;
                grade.GradedByUserId = gradedByUserId;
                // UpdatedAt gets stamped automatically — IAuditable + your SaveChangesAsync override
            }
            else
            {
                context.Grades.Add(new Grade
                {
                    EnrollmentId = enrollmentId,
                    SubjectId = subjectId,
                    MarksObtained = marks,
                    MaxMarks = maxMarks,
                    Remarks = remarks,
                    GradedByUserId = gradedByUserId,
                });
            }
        }

        await context.SaveChangesAsync();
    }

    public async Task<StudentReportCardDto?> GetReportCardAsync(Guid studentUserId, Guid classRoomId)
    {
        var enrollment = await context.Enrollments.AsNoTracking()
            .Include(e => e.StudentUser)
            .FirstOrDefaultAsync(e => e.StudentUserId == studentUserId && e.ClassRoomId == classRoomId);
        if (enrollment is null) return null;

        var subjects = await context.Subjects.AsNoTracking()
            .Where(s => s.ClassRoomId == classRoomId)
            .ToListAsync();

        var gradeBySubject = await context.Grades.AsNoTracking()
            .Where(g => g.EnrollmentId == enrollment.Id)
            .ToDictionaryAsync(g => g.SubjectId);

        return new StudentReportCardDto
        {
            StudentUserId = studentUserId,
            StudentName = enrollment.StudentUser.Email, // real name filled in by GradeService
            Subjects = subjects.Select(s => new SubjectGradeDto
            {
                SubjectName = s.Name,
                MarksObtained = gradeBySubject.TryGetValue(s.Id, out var g) ? g.MarksObtained : (decimal?)null,
                MaxMarks = gradeBySubject.TryGetValue(s.Id, out var g2) ? g2.MaxMarks : 100,
                Remarks = gradeBySubject.TryGetValue(s.Id, out var g3) ? g3.Remarks : null,
            }).ToList(),
        };
    }
}