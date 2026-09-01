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
}