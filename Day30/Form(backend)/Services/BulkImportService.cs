using ClosedXML.Excel;
using Form.DTOs;
using Form.Entities;
using Form.Interfaces;
using Form.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Form.Services;

public class BulkImportService(
    AppDbContext _context,
    IPasswordHasher _passwordHasher,
    ILogger<BulkImportService> _logger) : IBulkImportService
{
    // Serialises imports inside this app instance (double-click protection).
    // The unique index on Users.Email (Part B) is the guard that also works across servers.
    private static readonly SemaphoreSlim _importLock = new(1, 1);

    private const int MaxRows = 500;
    private const int MinPasswordLength = 8;

    // Compiled once, not rebuilt on every import.
    private static readonly Regex EmailRegex =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public async Task<BulkImportResultDto> ImportStudentsFromExcelAsync(IFormFile file, Guid classRoomId)
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

    private async Task<BulkImportResultDto> ImportInternalAsync(IFormFile file, Guid classRoomId)
    {
        var result = new BulkImportResultDto();

        // 1. Classroom must exist (the query filters do not apply to ClassRooms).
        if (!await _context.ClassRooms.AnyAsync(c => c.Id == classRoomId && !c.IsDeleted))
        {
            result.Errors.Add("Selected classroom was not found.");
            return result;
        }

        // 2. Parse the workbook. A corrupt file is the user's problem, not a server error.
        List<ParsedStudentRow> parsedRows;
        try
        {
            parsedRows = await ParseWorkbookAsync(file);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Bulk import: workbook could not be read");
            result.Errors.Add("The file could not be read. Use the downloaded template and save it as .xlsx.");
            return result;
        }

        if (parsedRows.Count == 0)
        {
            result.Errors.Add("The uploaded Excel spreadsheet is empty.");
            return result;
        }
        if (parsedRows.Count > MaxRows)
        {
            result.Errors.Add($"Too many rows ({parsedRows.Count}). Import at most {MaxRows} students at a time.");
            return result;
        }

        // 3. Validate everything before touching the database.
        //    Only look up the emails that are actually in the file (not the whole Users table).
        var fileEmails = parsedRows.Select(r => r.Email).Where(e => e.Length > 0).Distinct().ToList();
        var existingEmails = (await _context.Users
                .AsNoTracking()
                .Where(u => fileEmails.Contains(u.Email))
                .Select(u => u.Email)
                .ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var seenInFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var r in parsedRows)
        {
            if (string.IsNullOrWhiteSpace(r.FullName))
                result.Errors.Add($"Row {r.RowNumber}: Full Name is required.");
            else if (r.FullName.Length > 100)
                result.Errors.Add($"Row {r.RowNumber}: Full Name is longer than 100 characters.");

            if (string.IsNullOrWhiteSpace(r.Email) || r.Email.Length > 256 || !EmailRegex.IsMatch(r.Email))
            {
                result.Errors.Add($"Row {r.RowNumber}: Invalid email address '{r.Email}'.");
            }
            else
            {
                var firstTimeInFile = seenInFile.Add(r.Email);

                if (existingEmails.Contains(r.Email))
                    result.Errors.Add(
                        $"Row {r.RowNumber}: Email '{r.Email}' is already registered. " +
                        "If the account is deactivated, reactivate it first or use a different email.");
                else if (!firstTimeInFile)
                    result.Errors.Add($"Row {r.RowNumber}: Duplicate email '{r.Email}' found within the spreadsheet.");
            }

            if (r.TemporaryPassword.Length < MinPasswordLength || r.TemporaryPassword.Length > 72)
                result.Errors.Add(
                    $"Row {r.RowNumber}: temporary password must be {MinPasswordLength} to 72 characters.");
        }

        if (result.Errors.Count > 0)
        {
            result.Success = false;
            return result;
        }

        // 4. Hash all passwords in parallel BEFORE writing (BCrypt is deliberately slow).
        var hashes = new string[parsedRows.Count];
        await Task.Run(() => Parallel.For(0, parsedRows.Count,
            i => hashes[i] = _passwordHasher.Hash(parsedRows[i].TemporaryPassword)));

        // 5. One SaveChangesAsync = one database transaction: all rows are saved or none are.
        var now = DateTime.UtcNow;
        for (var i = 0; i < parsedRows.Count; i++)
        {
            var r = parsedRows[i];

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = r.Email,
                PasswordHash = hashes[i],
                CreatedAt = now,
                IsActive = true,
            };
            _context.Users.Add(user);

            _context.UserRoleAssignments.Add(new UserRoleAssignment
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Role = UserRole.Student,
            });

            // MemberNumber is DELIBERATELY not set: it is an IDENTITY column.
            _context.UserProfiles.Add(new UserProfile
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                FullName = r.FullName,
                Address = string.Empty,
                PhoneNumbers = new List<string>(),
                CreatedAt = now,
                UpdatedAt = now,
            });

            _context.Enrollments.Add(new Enrollment
            {
                Id = Guid.NewGuid(),
                StudentUserId = user.Id,
                ClassRoomId = classRoomId,
                EnrolledAt = now,
            });
        }

        try
        {
            await _context.SaveChangesAsync();
            result.Success = true;
            result.ImportedCount = parsedRows.Count;
            return result;
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            _context.ChangeTracker.Clear();
            result.Errors.Add("One of these emails was registered while the import was running. Nothing was imported; please try again.");
            return result;
        }
        catch (Exception ex)
        {
            _context.ChangeTracker.Clear();
            _logger.LogError(ex, "Bulk import failed for classroom {ClassRoomId}", classRoomId);
            result.Errors.Add("The import failed because of a server error. Nothing was imported.");   // no SQL text to the client
            return result;
        }
    }

    private static async Task<List<ParsedStudentRow>> ParseWorkbookAsync(IFormFile file)
    {
        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        stream.Position = 0;

        using var workbook = new XLWorkbook(stream);
        var worksheet = workbook.Worksheet(1);

        // Row 1 is the header. RowNumber() is the REAL Excel row, so error messages point at the right line
        // even when blank rows are skipped.
        return worksheet.RowsUsed()
            .Where(row => row.RowNumber() > 1)
            .Select(row => new ParsedStudentRow
            {
                RowNumber = row.RowNumber(),
                FullName = row.Cell(1).GetString().Trim(),
                Email = row.Cell(2).GetString().Trim(),
                TemporaryPassword = row.Cell(3).GetString().Trim(),
            })
            .ToList();
    }

    public byte[] GenerateSampleExcelTemplate()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Students");

        worksheet.Cell(1, 1).Value = "Full Name";
        worksheet.Cell(1, 2).Value = "Email Address";
        worksheet.Cell(1, 3).Value = "Temporary Password";

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = XLColor.FromArgb(79, 70, 229);
        headerRow.Style.Font.FontColor = XLColor.White;

        worksheet.Cell(2, 1).Value = "Sushit Shrestha";
        worksheet.Cell(2, 2).Value = "sushit@example.com";
        worksheet.Cell(2, 3).Value = "ChangeMe-2026";   // 8+ characters; give every student a different one

        worksheet.Cell(3, 1).Value = "Asta";
        worksheet.Cell(3, 2).Value = "Asta.Clover@example.com";
        worksheet.Cell(3, 3).Value = "ChangeMe-2027";

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}