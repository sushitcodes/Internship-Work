using Form.DTOs;
using Form.Entities;
using Form.Interfaces;

namespace Form.Services;

public class DashboardService(
    IUserRepository userRepository,
    ISubmissionRepository submissionRepository,
    IAttendanceRepository attendanceRepository,
    IConfiguration config) : IDashboardService
{
    // "Today" is the SCHOOL's today, not UTC's.
    private DateOnly SchoolToday()
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(config["School:TimeZone"] ?? "Asia/Kathmandu");
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz));
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        var today = SchoolToday();

        var totalStudents = await userRepository.CountByRoleAsync(UserRole.Student);
        var totalStaff = await userRepository.CountByRoleAsync(UserRole.Staff);
        var totalSubmissions = await submissionRepository.GetCountAsync();
        var stats = await attendanceRepository.GetTodayStatsAsync(today);
        var recent = await submissionRepository.GetRecentAsync(5);

        return new DashboardSummaryDto
        {
            TotalStudents = totalStudents,
            TotalStaff = totalStaff,
            TotalSubmissions = totalSubmissions,
            TodayAttendanceBreakdown = stats.Breakdown
                .Select(b => new StatusCountDto { Status = b.Status, Count = b.Count })
                .ToList(),
            RecentSubmissions = recent.Select(MapSubmission).ToList(),
        };
    }

    public async Task<MyDashboardDto> GetMyDashboardAsync(Guid userId)
    {
        var mySubmissionsCount = await submissionRepository.GetCountByUserAsync(userId);
        var myRecentSubmissions = await submissionRepository.GetRecentByUserAsync(userId, 5);

        // Counts happen in SQL; only the 5 rows we actually show are loaded.
        var (presentCount, totalCount) = await attendanceRepository.GetStudentCountsAsync(userId);
        var recentAttendance = await attendanceRepository.GetRecentByStudentAsync(userId, 5);

        var percentage = totalCount == 0 ? 0 : Math.Round(100.0 * presentCount / totalCount, 1);

        return new MyDashboardDto
        {
            MySubmissionsCount = mySubmissionsCount,
            MyRecentSubmissions = myRecentSubmissions.Select(MapSubmission).ToList(),
            MyAttendancePercentage = percentage,
            MyRecentAttendance = recentAttendance.Select(a => new AttendanceRecordDtos
            {
                Id = a.Id,
                EnrollmentId = a.EnrollmentId,
                StudentUserId = a.Enrollment.StudentUserId,
                StudentEmail = a.Enrollment.StudentUser.Email,
                Date = a.Date,
                Status = a.Status.ToString(),
            }).ToList(),
        };
    }

    private static SubmissionDto MapSubmission(Submission s) => new()
    {
        Id = s.Id,
        FullName = s.FullName,
        ClassRoomId = s.ClassRoomId,
        ClassRoomName = s.ClassRoom?.Name ?? string.Empty,
        RollNo = s.RollNo,
        FileUrl = s.FileUrl,
        CreatedAt = s.CreatedAt,
    };
}