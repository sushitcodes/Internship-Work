using Form.DTOs;
using Form.Entities;
using Form.Interfaces;
using static Form.DTOs.ClassRoomDtos; // SubmissionDto lives here, per your existing wrapper

namespace Form.Services;

public class DashboardService : IDashboardService
{
    private readonly IUserRepository _userRepository;
    private readonly ISubmissionRepository _submissionRepository;
    private readonly IAttendanceRepository _attendanceRepository;

    public DashboardService(
        IUserRepository userRepository,
        ISubmissionRepository submissionRepository,
        IAttendanceRepository attendanceRepository)
    {
        _userRepository = userRepository;
        _submissionRepository = submissionRepository;
        _attendanceRepository = attendanceRepository;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        var totalStudents = await _userRepository.CountByRoleAsync(UserRole.Student);
        var totalStaff = await _userRepository.CountByRoleAsync(UserRole.Staff);
        var totalSubmissions = await _submissionRepository.GetCountAsync();
        var (present, totalMarked) = await _attendanceRepository.GetTodaySummaryAsync(today);
        var recent = await _submissionRepository.GetRecentAsync(5);

        return new DashboardSummaryDto
        {
            TotalStudents = totalStudents,
            TotalStaff = totalStaff,
            TotalSubmissions = totalSubmissions,
            // Guard against divide-by-zero when nobody's marked attendance yet today.
            TodayAttendancePercentage = totalMarked == 0 ? 0 : Math.Round(100.0 * present / totalMarked, 1),
            RecentSubmissions = recent.Select(MapSubmission).ToList(),
        };
    }

    public async Task<MyDashboardDto> GetMyDashboardAsync(Guid userId)
    {
        var mySubmissionsCount = await _submissionRepository.GetCountByUserAsync(userId);
        var myRecentSubmissions = await _submissionRepository.GetRecentByUserAsync(userId, 5);
        var myAttendance = await _attendanceRepository.GetByStudentAsync(userId);

        var presentCount = myAttendance.Count(a => a.Status == Entities.AttendanceStatus.Present);
        var percentage = myAttendance.Count == 0 ? 0 : Math.Round(100.0 * presentCount / myAttendance.Count, 1);

        return new MyDashboardDto
        {
            MySubmissionsCount = mySubmissionsCount,
            MyRecentSubmissions = myRecentSubmissions.Select(MapSubmission).ToList(),
            MyAttendancePercentage = percentage,
            MyRecentAttendance = myAttendance.Take(5).Select(a => new AttendanceRecordDto
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
        Email = s.Email,
        Phone = s.Phone,
        FileUrl = s.FileUrl,
        CreatedAt = s.CreatedAt,
        Education = s.Education.Select(e => new EducationEntryDto
        {
            Institution = e.Institution,
            Degree = e.Degree,
            Year = e.Year,
        }).ToList(),
    };
}