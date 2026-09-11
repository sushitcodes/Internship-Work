using Form.DTOs;
using Form.Interfaces;
using Microsoft.AspNetCore.Mvc;
using static Form.DTOs.ClassRoomDtos;
using Microsoft.AspNetCore.Authorization;

namespace Form.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubmissionsController(ISubmissionService _submissionService) : ControllerBase
{
   

    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<SubmissionDto>> Create([FromForm] SubmissionFormRequest form)
    {
        if (form.File is null || form.File.Length == 0)
            return BadRequest("A file is required.");

        if (form.ClassRoomId == Guid.Empty)
            return BadRequest("ClassRoomId is required.");

        if (form.RollNo <= 0)
            return BadRequest("RollNo must be greater than 0.");

        // Get user ID from claims - must exist since [Authorize] is used
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var createdByUserId))
            return Unauthorized("User ID not found in token.");

        var request = new CreateSubmissionRequest
        {
            FullName = form.FullName,
            ClassRoomId = form.ClassRoomId,
            RollNo = form.RollNo,
            File = form.File,
            CreatedByUserId = createdByUserId, // Non-nullable Guid
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

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<PagedResult<SubmissionDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 50) pageSize = 10;

        return Ok(await _submissionService.GetPagedAsync(page, pageSize, search));
    }

    [HttpGet("count")]
    public async Task<ActionResult<int>> GetCount()
    {
        return Ok(await _submissionService.GetCountAsync());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SubmissionDto>> GetById(Guid id)
    {
        var result = await _submissionService.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "CanEdit")]
    public async Task<ActionResult<SubmissionDto>> Update(Guid id, [FromForm] SubmissionFormRequest form)
    {
        if (form.ClassRoomId == Guid.Empty)
            return BadRequest("ClassRoomId is required.");

        if (form.RollNo <= 0)
            return BadRequest("RollNo must be greater than 0.");

        var request = new UpdateSubmissionRequest
        {
            FullName = form.FullName,
            ClassRoomId = form.ClassRoomId,
            RollNo = form.RollNo,
            File = form.File,
        };

        var result = await _submissionService.UpdateAsync(id, request);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "CanDelete")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _submissionService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}

public class SubmissionFormRequest
{
    public string FullName { get; set; } = string.Empty;
    public Guid ClassRoomId { get; set; }
    public int RollNo { get; set; }
    public IFormFile? File { get; set; }
}