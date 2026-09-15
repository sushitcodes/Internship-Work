namespace Form.DTOs;

public class AttendanceRecordDtos
{
    public Guid Id { get; set; }
    public Guid EnrollmentId { get; set; }
    public Guid StudentUserId { get; set; }
    public string StudentEmail { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class MarkAttendanceEntry
{
    public Guid EnrollmentId { get; set; }
    public string Status { get; set; } = "Present";
}

public class MarkAttendanceRequest
{
    public Guid ClassRoomId { get; set; }
    public DateOnly Date { get; set; }
    public List<MarkAttendanceEntry> Entries { get; set; } = new();
}
public class AttendanceRosterEntryDto
{
    public Guid EnrollmentId { get; set; }
    public Guid StudentUserId { get; set; }
    public string StudentEmail { get; set; } = string.Empty;
    public Guid? AttendanceRecordId { get; set; }
    public string Status { get; set; } = "Unmarked";
}
public class AttendanceSheetRowDto
{
    public Guid EnrollmentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    // Keyed by "yyyy-MM-dd" so the frontend can look up any date directly
    // without re-parsing DateOnly on every cell render.
    public Dictionary<string, string> StatusByDate { get; set; } = new();
}

public class AttendanceSheetDto
{
    public List<string> Dates { get; set; } = new(); // "yyyy-MM-dd", in order
    public List<AttendanceSheetRowDto> Rows { get; set; } = new();
}