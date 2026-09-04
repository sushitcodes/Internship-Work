namespace Form.DTOs;

public class AttendanceRecordDto
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