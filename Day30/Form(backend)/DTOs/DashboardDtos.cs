using static Form.DTOs.ClassRoomDtos;

namespace Form.DTOs;

public class DashboardSummaryDto
{
    public int TotalStudents { get; set; }
    public int TotalStaff { get; set; }
    public int TotalSubmissions { get; set; }
    public double TodayAttendancePercentage { get; set; } // 0 when nothing marked yet today
    public List<SubmissionDto> RecentSubmissions { get; set; } = new();
}

public class MyDashboardDto
{
    public int MySubmissionsCount { get; set; }
    public List<SubmissionDto> MyRecentSubmissions { get; set; } = new();
    public double MyAttendancePercentage { get; set; } // across all their own marked records, all-time
    public List<AttendanceRecordDto> MyRecentAttendance { get; set; } = new();
}