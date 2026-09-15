namespace Form.DTOs;

public class SubjectDto
{
    public Guid Id { get; set; }
    public Guid ClassRoomId { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class CreateSubjectRequest
{
    public Guid ClassRoomId { get; set; }
    public string Name { get; set; } = string.Empty;
}

// One row per student on the "enter grades" screen — same shape idea as
// AttendanceRosterEntryDto, just no grade yet instead of "Unmarked".
public class GradeRosterEntryDto
{
    public Guid EnrollmentId { get; set; }
    public Guid StudentUserId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public Guid? GradeId { get; set; }
    public decimal? MarksObtained { get; set; }
    public decimal MaxMarks { get; set; } = 100;
    public string? Remarks { get; set; }
}

public class SubmitGradeEntry
{
    public Guid EnrollmentId { get; set; }
    public decimal MarksObtained { get; set; }
    public decimal MaxMarks { get; set; } = 100;
    public string? Remarks { get; set; }
}

public class SubmitGradesRequest
{
    public Guid SubjectId { get; set; }
    public List<SubmitGradeEntry> Entries { get; set; } = new();
}

public class SubjectGradeDto
{
    public string SubjectName { get; set; } = string.Empty;
    public decimal? MarksObtained { get; set; }
    public decimal MaxMarks { get; set; } = 100;
    public string? Remarks { get; set; }
}

public class StudentReportCardDto
{
    public Guid StudentUserId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public List<SubjectGradeDto> Subjects { get; set; } = new();
}