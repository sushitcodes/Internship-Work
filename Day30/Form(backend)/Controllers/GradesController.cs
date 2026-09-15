using Form.DTOs;
using Form.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Form.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GradesController : ControllerBase
{
    private readonly IGradeService _gradeService;
    public GradesController(IGradeService gradeService) => _gradeService = gradeService;

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
}