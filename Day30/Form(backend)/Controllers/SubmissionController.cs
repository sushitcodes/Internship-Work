using Form.DTOs;
using Form.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Form.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubmissionsController(ISubmissionService _submissionService) : ControllerBase
{
    private bool IsStaffOrAdmin => User.IsInRole("Staff") || User.IsInRole("Admin");

    private Guid? CurrentUserId =>
        Guid.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id) ? id : null;

    // null means "no restriction" (staff). A student gets their own id, so the query filters to their rows.
    private Guid? ScopeFilter => IsStaffOrAdmin ? null : CurrentUserId ?? Guid.Empty;

    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<SubmissionDto>> Create([FromForm] CreateSubmissionFormRequest form)
    {
        if (CurrentUserId is not { } createdByUserId)
            return Unauthorized("User ID not found in token.");

        var request = new CreateSubmissionRequest
        {
            FullName = form.FullName,
            ClassRoomId = form.ClassRoomId,
            RollNo = form.RollNo,
            File = form.File!,
            CreatedByUserId = createdByUserId,
            CreatedByStaff = IsStaffOrAdmin,
        };

        try
        {
            var result = await _submissionService.CreateSubmissionAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<SubmissionDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 50) pageSize = 10;

        return Ok(await _submissionService.GetPagedAsync(page, pageSize, search, ScopeFilter));
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetCount() =>
        Ok(await _submissionService.GetCountAsync(ScopeFilter));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SubmissionDto>> GetById(Guid id)
    {
        var result = await _submissionService.GetByIdAsync(id);
        if (result is null) return NotFound();

        // A student can only open their own submission. 404 (not 403) so the id is not confirmed to exist.
        if (!IsStaffOrAdmin && result.CreatedByUserId != CurrentUserId)
            return NotFound();

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CanEdit")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<SubmissionDto>> Update(Guid id, [FromForm] UpdateSubmissionFormRequest form)
    {
        var request = new UpdateSubmissionRequest
        {
            FullName = form.FullName,
            ClassRoomId = form.ClassRoomId,
            RollNo = form.RollNo,
            File = form.File,
        };

        try
        {
            var result = await _submissionService.UpdateAsync(id, request);
            return result is null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)   // was missing: a bad file type used to return 500
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "CanDelete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _submissionService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}