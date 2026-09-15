using Form.DTOs;
using Form.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Form.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AttendanceController(IAttendanceService _attendanceService) : ControllerBase
{


    [HttpPost("mark")]
    [Authorize(Policy = "StaffOrAdmin")]
    public async Task<IActionResult> Mark(MarkAttendanceRequest request)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var markedByUserId))
            return Unauthorized();

        try
        {
            await _attendanceService.MarkAsync(request, markedByUserId);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("class/{classRoomId:guid}")]
    [Authorize(Policy = "StaffOrAdmin")]
    public async Task<ActionResult<List<AttendanceRecordDtos>>> GetForClass(
        Guid classRoomId, [FromQuery] DateOnly date) =>
        Ok(await _attendanceService.GetForClassAsync(classRoomId, date));

    [HttpGet("me")]
    public async Task<ActionResult<List<AttendanceRecordDtos>>> GetMine()
    {
        // Deliberately reads the CALLER's own ID from the token — no
        // studentUserId parameter accepted here at all. This is what
        // makes "Students only see their own" a structural guarantee,
        // not a check that could be bypassed by changing a query param.
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var studentUserId))
            return Unauthorized();

        return Ok(await _attendanceService.GetForStudentAsync(studentUserId));
    }
    [HttpGet("roster/{classRoomId:guid}")]
    [Authorize(Policy = "StaffOrAdmin")]
    public async Task<ActionResult<List<AttendanceRosterEntryDto>>> GetRoster(
    Guid classRoomId, [FromQuery] DateOnly date) =>
    Ok(await _attendanceService.GetRosterAsync(classRoomId, date));
    [HttpGet("sheet/{classRoomId:guid}")]
    [Authorize(Policy = "StaffOrAdmin")]
    public async Task<ActionResult<AttendanceSheetDto>> GetSheet(
    Guid classRoomId, [FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
    {
        if (startDate > endDate) return BadRequest("Start date must be before end date.");
        return Ok(await _attendanceService.GetSheetAsync(classRoomId, startDate, endDate));
    }

    [HttpGet("sheet/{classRoomId:guid}/export")]
    [Authorize(Policy = "StaffOrAdmin")]
    public async Task<IActionResult> ExportSheet(
        Guid classRoomId, [FromQuery] DateOnly startDate, [FromQuery] DateOnly endDate)
    {
        if (startDate > endDate) return BadRequest("Start date must be before end date.");
        var bytes = await _attendanceService.ExportSheetAsync(classRoomId, startDate, endDate);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"attendance_{startDate:yyyy-MM-dd}_to_{endDate:yyyy-MM-dd}.xlsx");
    }
}