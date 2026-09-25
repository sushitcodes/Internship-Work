using ClosedXML.Excel;
using Form.DTOs;
using Form.Entities;
using Form.Interfaces;
using Form.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Form.Services;

public class BulkImportService : IBulkImportService
{
    // Serializes imports within this app instance. Stops the classic
    // "double-click Start Import" race where two requests both pass the
    // email-uniqueness check and one blows up at SaveChangesAsync.
    // If you ever scale to multiple servers, this becomes advisory only —
    // the real guard is a unique index on Users.Email (see notes below).
    private static readonly SemaphoreSlim _importLock = new(1, 1);

    private readonly AppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public BulkImportService(AppDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<BulkImportResultDto> ImportStudentsFromExcelAsync(
        IFormFile file, Guid classRoomId)
    {
        await _importLock.WaitAsync();
        try
        {
            return await ImportInternalAsync(file, classRoomId);
        }
        finally
        {
            _importLock.Release();
        }
    }

    private async Task<BulkImportResultDto> ImportInternalAsync(
        IFormFile file, Guid classRoomId)
    {
        var result = new BulkImportResultDto();

        // 1. Verify classroom exists
        var classroom = await _context.ClassRooms.FindAsync(classRoomId);
        if (classroom is null)
        {
            result.Errors.Add("Selected classroom was not found.");
            return result;
        }

        // 2. Parse Excel into memory
        var parsedRows = new List<ParsedStudentRow>();
        using (var stream = new MemoryStream())
        {
            await file.CopyToAsync(stream);

            using var workbook = new XLWorkbook(stream);
            var worksheet = workbook.Worksheet(1);

            // Skip the header row. Now only 3 columns: Full Name, Email, Temp Password.
            var rows = worksheet.RangeUsed()?.RowsUsed().Skip(1);
            if (rows is null || !rows.Any())
            {
                result.Errors.Add("The uploaded Excel spreadsheet is empty.");
                return result;
            }

            int rowNumber = 2; // header is row 1
            foreach (var row in rows)
            {
                var fullName = row.Cell(1).GetString().Trim();
                var email = row.Cell(2).GetString().Trim();
                var tempPass = row.Cell(3).GetString().Trim();

                parsedRows.Add(new ParsedStudentRow
                {
                    RowNumber = rowNumber++,
                    FullName = fullName,
                    Email = email,
                    TemporaryPassword = string.IsNullOrWhiteSpace(tempPass)
                        ? "Student@123"
                        : tempPass,
                });
            }
        }

        // 3. Validation phase — everything checked before the transaction opens.
        var emailRegex = new Regex(
            @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
            RegexOptions.IgnoreCase);

        var seenEmailsInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Stale-by-a-millisecond is fine — the SemaphoreSlim closes the
        // obvious races, and a unique index on Users.Email would close the rest.
        var existingDbEmails = await _context.Users
            .Select(u => u.Email.ToLower())
            .ToListAsync();

        foreach (var r in parsedRows)
        {
            if (string.IsNullOrWhiteSpace(r.FullName))
                result.Errors.Add($"Row {r.RowNumber}: Full Name is required.");

            if (string.IsNullOrWhiteSpace(r.Email) || !emailRegex.IsMatch(r.Email))
                result.Errors.Add($"Row {r.RowNumber}: Invalid email address '{r.Email}'.");

            if (existingDbEmails.Contains(r.Email.ToLower()))
                result.Errors.Add(
                    $"Row {r.RowNumber}: Email '{r.Email}' is already registered in the system.");

            if (!seenEmailsInFile.Add(r.Email))
                result.Errors.Add(
                    $"Row {r.RowNumber}: Duplicate email '{r.Email}' found within the spreadsheet.");
        }

        if (result.Errors.Count > 0)
        {
            result.Success = false;
            return result;
        }

        // 4. Execution phase — single transaction, all-or-nothing.
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var r in parsedRows)
            {
                // A. User account
                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Email = r.Email,
                    PasswordHash = _passwordHasher.Hash(r.TemporaryPassword),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                };
                _context.Users.Add(user);

                // B. Student role
                _context.UserRoleAssignments.Add(new UserRoleAssignment
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    Role = UserRole.Student,
                });

                // C. Profile — MemberNumber is DELIBERATELY not set.
                //    It's an IDENTITY column; SQL Server assigns the next
                //    sequential value on insert, exactly like every other
                //    profile-creation path in this app.
                _context.UserProfiles.Add(new UserProfile
                {
                    Id = Guid.NewGuid(),
                    UserId = user.Id,
                    FullName = r.FullName,
                    Address = string.Empty,
                    PhoneNumbers = new List<string>(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                });

                // D. Classroom enrollment
                _context.Enrollments.Add(new Enrollment
                {
                    Id = Guid.NewGuid(),
                    StudentUserId = user.Id,
                    ClassRoomId = classRoomId,
                    EnrolledAt = DateTime.UtcNow,
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            result.Success = true;
            result.ImportedCount = parsedRows.Count;
            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            result.Success = false;
            result.Errors.Add($"Database error during import: {ex.Message}");
            return result;
        }
    }

    public byte[] GenerateSampleExcelTemplate()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Students");

        // Three columns now — no Roll Number.
        worksheet.Cell(1, 1).Value = "Full Name";
        worksheet.Cell(1, 2).Value = "Email Address";
        worksheet.Cell(1, 3).Value = "Temporary Password";

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.FromArgb(79, 70, 229);
        headerRow.Style.Font.FontColor = XLColor.White;

        worksheet.Cell(2, 1).Value = "Sushit Shrestha";
        worksheet.Cell(2, 2).Value = "sushit@example.com";
        worksheet.Cell(2, 3).Value = "123456789";

        worksheet.Cell(3, 1).Value = "Asta";
        worksheet.Cell(3, 2).Value = "Asta.Clover@example.com";
        worksheet.Cell(3, 3).Value = "123456789";

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}