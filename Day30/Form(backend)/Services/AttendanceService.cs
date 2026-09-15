using ClosedXML.Excel;
using Form.DTOs;
using Form.Entities;
using Form.Interfaces;
namespace Form.Services;

public class AttendanceService : IAttendanceService
{
    private readonly IAttendanceRepository _repository;
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly IUserProfileRepository _profileRepository;
    public AttendanceService(
           IAttendanceRepository repository,
           IEnrollmentRepository enrollmentRepository,
           IUserProfileRepository profileRepository)
    {
        _repository = repository;
        _enrollmentRepository = enrollmentRepository;
        _profileRepository = profileRepository;
    }
    public async Task<List<AttendanceRosterEntryDto>> GetRosterAsync(Guid classRoomId, DateOnly date) =>
        await _repository.GetRosterAsync(classRoomId, date);

    public async Task MarkAsync(MarkAttendanceRequest request, Guid markedByUserId)
    {
        // No more ExistsForDateAsync guard — marking today never blocks
        // marking (or re-marking) today again. That guard was the whole bug.
        var entries = request.Entries
            .Select(e => (e.EnrollmentId, Enum.Parse<AttendanceStatus>(e.Status)))
            .ToList();

        await _repository.UpsertRangeAsync(request.ClassRoomId, request.Date, entries, markedByUserId);
    }

    public async Task<List<AttendanceRecordDtos>> GetForClassAsync(Guid classRoomId, DateOnly date) =>
        (await _repository.GetByClassRoomAndDateAsync(classRoomId, date)).Select(MapToDto).ToList();

    public async Task<List<AttendanceRecordDtos>> GetForStudentAsync(Guid studentUserId) =>
        (await _repository.GetByStudentAsync(studentUserId)).Select(MapToDto).ToList();

    private static AttendanceRecordDtos MapToDto(AttendanceRecord a) => new()
    {
        Id = a.Id,
        EnrollmentId = a.EnrollmentId,
        StudentUserId = a.Enrollment.StudentUserId,
        StudentEmail = a.Enrollment.StudentUser.Email,
        Date = a.Date,
        Status = a.Status.ToString(),
    };

    public async Task<AttendanceSheetDto> GetSheetAsync(Guid classRoomId, DateOnly start, DateOnly end)
    {
        var enrollments = await _enrollmentRepository.GetByClassRoomIdAsync(classRoomId);
        var records = await _repository.GetByClassRoomAndDateRangeAsync(classRoomId, start, end);
        var profiles = await _profileRepository.GetByUserIdsAsync(enrollments.Select(e => e.StudentUserId).ToList());
        var nameByUserId = profiles.ToDictionary(p => p.UserId, p => p.FullName);

        var dates = new List<DateOnly>();
        for (var d = start; d <= end; d = d.AddDays(1)) dates.Add(d);

        var recordLookup = records.ToDictionary(r => (r.EnrollmentId, r.Date));

        var rows = enrollments.Select(e => new AttendanceSheetRowDto
        {
            EnrollmentId = e.Id,
            // Falls back to email ONLY if the student never opened their
            // profile yet — same lazy-profile fallback pattern as UsersListPage.
            StudentName = nameByUserId.TryGetValue(e.StudentUserId, out var name) ? name : e.StudentUser.Email,
            StatusByDate = dates.ToDictionary(
                d => d.ToString("yyyy-MM-dd"),
                d => recordLookup.TryGetValue((e.Id, d), out var rec) ? rec.Status.ToString() : "Unmarked"),
        })
        .OrderBy(r => r.StudentName)
        .ToList();

        return new AttendanceSheetDto
        {
            Dates = dates.Select(d => d.ToString("yyyy-MM-dd")).ToList(),
            Rows = rows,
        };
    }

    public async Task<byte[]> ExportSheetAsync(Guid classRoomId, DateOnly start, DateOnly end)
    {
        var sheet = await GetSheetAsync(classRoomId, start, end);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Attendance");

        ws.Cell(1, 1).Value = "Student";
        for (int i = 0; i < sheet.Dates.Count; i++)
            ws.Cell(1, i + 2).Value = sheet.Dates[i];

        for (int row = 0; row < sheet.Rows.Count; row++)
        {
            var r = sheet.Rows[row];
            ws.Cell(row + 2, 1).Value = r.StudentName;
            for (int col = 0; col < sheet.Dates.Count; col++)
                ws.Cell(row + 2, col + 2).Value = r.StatusByDate[sheet.Dates[col]];
        }

        ws.Columns().AdjustToContents();
        ws.Row(1).Style.Font.Bold = true;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
