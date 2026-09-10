using Form.DTOs;
using Form.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Form.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;
    public EnrollmentsController(IEnrollmentService enrollmentService) => _enrollmentService = enrollmentService;

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<EnrollmentDto>> Enroll(CreateEnrollmentRequest request)
    {
        try
        {
            return Ok(await _enrollmentService.EnrollAsync(request));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("class/{classRoomId:guid}")]
    [Authorize(Policy = "StaffOrAdmin")]
    public async Task<ActionResult<List<EnrollmentDto>>> GetByClassRoom(Guid classRoomId) =>
        Ok(await _enrollmentService.GetByClassRoomAsync(classRoomId));
   
    [HttpDelete("{studentUserId:guid}/class/{classRoomId:guid}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Remove(Guid studentUserId, Guid classRoomId)
    {
        var removed = await _enrollmentService.RemoveAsync(studentUserId, classRoomId);
        return removed ? NoContent() : NotFound();
    }
    [HttpGet("me/class")]
    public async Task<ActionResult<EnrollmentDto>> GetMyClass()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId)) return Unauthorized();

        var result = await _enrollmentService.GetMyEnrollmentAsync(userId);
        return result is null ? NotFound() : Ok(result);
    }


}