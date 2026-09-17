namespace Form.DTOs;

public class StatusCountDto
{
    public string Status { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class DashboardSummaryDto
{
    public int TotalStudents { get; set; }
    public int TotalStaff { get; set; }
    public int TotalSubmissions { get; set; }
    public List<StatusCountDto> TodayAttendanceBreakdown { get; set; } = new(); // replaces TodayAttendancePercentage
    public List<SubmissionDto> RecentSubmissions { get; set; } = new();
}
public class MyDashboardDto
{
    public int MySubmissionsCount { get; set; }
    public List<SubmissionDto> MyRecentSubmissions { get; set; } = new();
    public double MyAttendancePercentage { get; set; } // across all their own marked records, all-time
    public List<AttendanceRecordDtos> MyRecentAttendance { get; set; } = new();
}