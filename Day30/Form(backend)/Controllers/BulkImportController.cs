using Form.DTOs;
using Form.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Form.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class BulkImportController(IBulkImportService bulkImportService) : ControllerBase
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    [HttpGet("template")]
    public IActionResult DownloadTemplate()
    {
        var fileBytes = bulkImportService.GenerateSampleExcelTemplate();
        return File(
            fileBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Student_Import_Template.xlsx");
    }

    [HttpPost("students")]
    [RequestSizeLimit(MaxFileSizeBytes)]
    public async Task<ActionResult<BulkImportResultDto>> ImportStudents(
        [FromForm] IFormFile file,
        [FromForm] Guid classRoomId)
    {
        if (file is null || file.Length == 0)
            return BadRequest("Please upload an Excel (.xlsx) file.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext != ".xlsx")
            return BadRequest("Only .xlsx files are accepted.");

        if (file.Length > MaxFileSizeBytes)
            return BadRequest("File too large. Max 10 MB.");

        var result = await bulkImportService.ImportStudentsFromExcelAsync(
            file, classRoomId);

        return Ok(result);
    }
}