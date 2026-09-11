using Form.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Form.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DashboardController(IDashboardService _dashboardService) : ControllerBase
{
    

    [HttpGet("summary")]
    [Authorize(Policy = "StaffOrAdmin")]
    public async Task<IActionResult> GetSummary() => Ok(await _dashboardService.GetSummaryAsync());

    [HttpGet("me")]
    public async Task<IActionResult> GetMine()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId)) return Unauthorized();

        return Ok(await _dashboardService.GetMyDashboardAsync(userId));
    }
}