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
    // CHECK 1: Get today's date according to the school's timezone.
    // Use the configured timezone, or default to Asia/Kathmandu.
    // Convert the current UTC time into the school's local time.
    private DateOnly SchoolToday()
    {
        var tz = TimeZoneInfo.FindSystemTimeZoneById(
            config["School:TimeZone"] ?? "Asia/Kathmandu");

        return DateOnly.FromDateTime(
            TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz));
    }

    // METHOD 1: Get dashboard summary for staff or administrators.
    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        // STEP 1: Get today's date in the school's timezone.
        var today = SchoolToday();

        // STEP 2: Count all users assigned the Student role.
        var totalStudents = await userRepository.CountByRoleAsync(UserRole.Student);

        // STEP 3: Count all users assigned the Staff role.
        var totalStaff = await userRepository.CountByRoleAsync(UserRole.Staff);

        // STEP 4: Get the total number of submissions.
        var totalSubmissions = await submissionRepository.GetCountAsync();

        // STEP 5: Get today's attendance statistics.
        var stats = await attendanceRepository.GetTodayStatsAsync(today);

        // STEP 6: Retrieve the five most recent submissions.
        var recent = await submissionRepository.GetRecentAsync(5);

        // STEP 7: Build and return the dashboard response DTO.
        return new DashboardSummaryDto
        {
            TotalStudents = totalStudents,
            TotalStaff = totalStaff,
            TotalSubmissions = totalSubmissions,

            // Convert attendance statistics into DTOs for the API response.
            TodayAttendanceBreakdown = stats.Breakdown
                .Select(b => new StatusCountDto
                {
                    Status = b.Status,
                    Count = b.Count
                })
                .ToList(),

            // Convert recent submissions into DTOs using the mapping method.
            RecentSubmissions = recent.Select(MapSubmission).ToList(),
        };
    }

    // METHOD 2: Get dashboard information for a specific student.
    public async Task<MyDashboardDto> GetMyDashboardAsync(Guid userId)
    {
        // STEP 1: Count submissions belonging to this user.
        var mySubmissionsCount =
            await submissionRepository.GetCountByUserAsync(userId);

        // STEP 2: Retrieve the user's five most recent submissions.
        var myRecentSubmissions =
            await submissionRepository.GetRecentByUserAsync(userId, 5);

        // STEP 3: Get the number of times the student was present
        // and the total attendance records. Counts are calculated in SQL.
        var (presentCount, totalCount) =
            await attendanceRepository.GetStudentCountsAsync(userId);

        // STEP 4: Retrieve the student's five most recent attendance records.
        var recentAttendance =
            await attendanceRepository.GetRecentByStudentAsync(userId, 5);

        // STEP 5: Calculate attendance percentage.
        // If there are no attendance records, return 0 to avoid division by zero.
        // Otherwise, calculate the percentage and round it to one decimal place.
        var percentage = totalCount == 0
            ? 0
            : Math.Round(100.0 * presentCount / totalCount, 1);

        // STEP 6: Build and return the student's dashboard response DTO.
        return new MyDashboardDto
        {
            MySubmissionsCount = mySubmissionsCount,

            // Convert the student's recent submissions into DTOs.
            MyRecentSubmissions = myRecentSubmissions
                .Select(MapSubmission)
                .ToList(),

            // Include the calculated attendance percentage.
            MyAttendancePercentage = percentage,

            // Convert attendance entities into response DTOs.
            MyRecentAttendance = recentAttendance.Select(a => new AttendanceRecordDtos
            {
                Id = a.Id,
                EnrollmentId = a.EnrollmentId,

                // Get the student's user ID through the enrollment relationship.
                StudentUserId = a.Enrollment.StudentUserId,

                // Get the student's email through the related user entity.
                StudentEmail = a.Enrollment.StudentUser.Email,

                // Include the attendance date.
                Date = a.Date,

                // Convert the attendance status enum into a string.
                Status = a.Status.ToString(),
            }).ToList(),
        };
    }

    // METHOD 3: Convert a Submission entity into a SubmissionDto.
    // This keeps entity-to-DTO mapping consistent across dashboard methods.
    private static SubmissionDto MapSubmission(Submission s) => new()
    {
        Id = s.Id,
        FullName = s.FullName,
        ClassRoomId = s.ClassRoomId,

        // Use an empty string if the related classroom is unavailable.
        ClassRoomName = s.ClassRoom?.Name ?? string.Empty,

        RollNo = s.RollNo,
        FileUrl = s.FileUrl,
        CreatedAt = s.CreatedAt,
    };
}