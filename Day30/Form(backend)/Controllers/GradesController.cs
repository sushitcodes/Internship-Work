using Form.DTOs;
using Form.Interface;
using Form.Interfaces;
using Form.Persistence.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Form.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GradesController(IGradeService _gradeService, IClassRoomRepository classRoomRepository, IReportCardPdfService pdfService) : ControllerBase
{
    [HttpGet("roster/{subjectId:guid}")]
    [Authorize(Policy = "StaffOrAdmin")]
    public async Task<ActionResult<List<GradeRosterEntryDto>>> GetRoster(Guid subjectId) =>
        Ok(await _gradeService.GetRosterAsync(subjectId));

    [HttpPost("submit")]
    [Authorize(Policy = "StaffOrAdmin")]
    public async Task<IActionResult> Submit(SubmitGradesRequest request)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var gradedByUserId))
            return Unauthorized();

        await _gradeService.SubmitAsync(request, gradedByUserId);
        return Ok();
    }

    // Same structural guarantee as AttendanceController.GetMine — the student
    // ID comes from the token, never from the URL, so there's no way to pass
    // someone else's ID and see their grades.
    [HttpGet("report-card/me/{classRoomId:guid}")]
    public async Task<ActionResult<StudentReportCardDto>> GetMyReportCard(Guid classRoomId)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var studentUserId))
            return Unauthorized();

        var card = await _gradeService.GetReportCardAsync(studentUserId, classRoomId);
        return card is null ? NotFound() : Ok(card);
    }

    [HttpGet("report-card/{studentUserId:guid}/{classRoomId:guid}")]
    [Authorize(Policy = "StaffOrAdmin")]
    public async Task<ActionResult<StudentReportCardDto>> GetReportCard(Guid studentUserId, Guid classRoomId)
    {
        var card = await _gradeService.GetReportCardAsync(studentUserId, classRoomId);
        return card is null ? NotFound() : Ok(card);
    }
    // it helps to get the student report by verifying with the jwt cookie claim
    [HttpGet("report-card/me/{classRoomId:guid}/pdf")]
    public async Task<IActionResult> DownloadMyReportCardPdf(Guid classRoomId)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var studentUserId))
            return Unauthorized();
        var card = await _gradeService.GetReportCardAsync(studentUserId, classRoomId);
        if (card is null) return NotFound("Report card data not found.");
        var classrooms = await classRoomRepository.GetAllAsync();
        var className = classrooms.FirstOrDefault(c => c.Id == classRoomId)?.Name ?? "Classroom";
        var pdfBytes = pdfService.GenerateReportCardPdf(card, className);
        var fileName = $"ReportCard_{card.StudentName.Replace(" ", "_")}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }
    //allow to generate report cards for the student.
    [HttpGet("report-card/{studentUserId:guid}/{classRoomId:guid}/pdf")]
    [Authorize(Policy = "StaffOrAdmin")]
    public async Task<IActionResult> DownloadReportCardPdf(Guid studentUserId, Guid classRoomId)
    {
        var card = await _gradeService.GetReportCardAsync(studentUserId, classRoomId);
        if (card is null) return NotFound("Report card data not found.");
        var classrooms = await classRoomRepository.GetAllAsync();
        var className = classrooms.FirstOrDefault(c => c.Id == classRoomId)?.Name ?? "Classroom";
        var pdfBytes = pdfService.GenerateReportCardPdf(card, className);
        var fileName = $"ReportCard_{card.StudentName.Replace(" ", "_")}.pdf";
        return File(pdfBytes, "application/pdf", fileName);
    }



}