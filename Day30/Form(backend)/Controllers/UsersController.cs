using Form.DTOs;
using Form.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Form.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // baseline: must be logged in — specific actions below tighten this further
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IUserProfileService _profileService;

    public UsersController(IUserService userService, IUserProfileService profileService)
    {
        _userService = userService;
        _profileService = profileService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

    private string CurrentUserEmail =>
        User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? User.Identity!.Name!;

    [HttpGet("students")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<List<UserSummaryDtos>>> GetStudents() =>
        Ok(await _userService.GetStudentsAsync());

    [HttpGet("me/profile")]
    public async Task<ActionResult<UserProfileDto>> GetOwnProfile()
    {
        var profile = await _profileService.GetOrCreateOwnProfileAsync(CurrentUserId, CurrentUserEmail);
        return Ok(profile);
    }

    [HttpPut("me/profile")]
    public async Task<ActionResult<UserProfileDto>> UpdateOwnProfile([FromForm] UpdateOwnProfileRequest request)
    {
        try
        {
            var updated = await _profileService.UpdateOwnProfileAsync(CurrentUserId, request);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    [Authorize(Policy = "StaffOrAdmin")]
    public async Task<ActionResult<PagedResult<UserProfileDto>>> Search(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] string? search = null,
    [FromQuery] int? rollNo = null,
    [FromQuery] string? role = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 50) pageSize = 10;

        try
        {
            return Ok(await _profileService.SearchAsync(page, pageSize, search, rollNo, role));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id:guid}/profile")]
    [Authorize(Policy = "StaffOrAdmin")]
    public async Task<ActionResult<UserProfileDto>> GetById(Guid id)
    {
        var profile = await _profileService.GetByUserIdAsync(id);
        return profile is null ? NotFound() : Ok(profile);
    }
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<CreatedUserDto>> CreateUser(CreateUserRequest request)
    {
        try
        {
            var result = await _userService.CreateUserAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
    [HttpPatch("{id:guid}/name")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<UserProfileDto>> UpdateName(Guid id, UpdateUserNameRequest request)
    {
        try
        {
            var updated = await _profileService.AdminUpdateNameAsync(id, request.FullName);
            return updated is null ? NotFound() : Ok(updated);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> SetActiveStatus(Guid id, SetUserActiveRequest request)
    {
        await _profileService.SetActiveStatusAsync(id, request.IsActive);
        return NoContent();
    }
}