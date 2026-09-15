using Form.DTOs;
using Form.Entities;
using Form.Interfaces;
using static Form.DTOs.ClassRoomDtos; // SubmissionDto lives here, per your existing wrapper

namespace Form.Services;

public class DashboardService(IUserRepository userRepository, ISubmissionRepository submissionRepository,
    IAttendanceRepository attendanceRepository
    
    ) : IDashboardService
{
    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

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
            // Guard against divide-by-zero when nobody's marked attendance yet today.
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
        var myAttendance = await attendanceRepository.GetByStudentAsync(userId);

        var presentCount = myAttendance.Count(a => a.Status == Entities.AttendanceStatus.Present);
        var percentage = myAttendance.Count == 0 ? 0 : Math.Round(100.0 * presentCount / myAttendance.Count, 1);

        return new MyDashboardDto
        {
            MySubmissionsCount = mySubmissionsCount,
            MyRecentSubmissions = myRecentSubmissions.Select(MapSubmission).ToList(),
            MyAttendancePercentage = percentage,
            MyRecentAttendance = myAttendance.Take(5).Select(a => new AttendanceRecordDtos
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