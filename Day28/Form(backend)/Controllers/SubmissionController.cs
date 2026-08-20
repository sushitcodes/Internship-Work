// SubmissionsController.cs
using Form.DTOs;
using Form.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using static Form.Dtos.Class;

namespace Form.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SubmissionsController : ControllerBase
{
    private readonly ISubmissionService _submissionService;

    public SubmissionsController(ISubmissionService submissionService)
    {
        _submissionService = submissionService;
    }

    [HttpPost]
    [RequestSizeLimit(10 * 1024 * 1024)] // hard cap enforced by the ASP.NET pipeline itself
    public async Task<ActionResult<SubmissionDto>> Create([FromForm] SubmissionFormRequest form)
    {
     
        if (form.File is null || form.File.Length == 0)
            return BadRequest("A file is required.");

        var education = ParseEducation(form.Education, out var parseError);
        if (parseError is not null) return BadRequest(parseError);

        var request = new CreateSubmissionRequest
        {
            FullName = form.FullName,
            Email = form.Email,
            Phone = form.Phone,
            Education = education!,
            File = form.File
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
        // Guard rails against a malformed or malicious query string
        // (e.g. ?pageSize=999999 trying to force-dump the whole table).
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 50) pageSize = 10;

        return Ok(await _submissionService.GetPagedAsync(page, pageSize, search));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SubmissionDto>> GetById(Guid id)
    {
        var result = await _submissionService.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SubmissionDto>> Update(Guid id, [FromForm] SubmissionFormRequest form)
    {
        
        var education = ParseEducation(form.Education, out var parseError);
        if (parseError is not null) return BadRequest(parseError);

        var request = new UpdateSubmissionRequest
        {
            FullName = form.FullName,
            Email = form.Email,
            Phone = form.Phone,
            Education = education!,
            File = form.File, // may genuinely be null on this action
        };

        var result = await _submissionService.UpdateAsync(id, request);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _submissionService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    
    private static List<CreateEducationEntryRequest>? ParseEducation(string json, out string? error)
    {
        try
        {
            error = null;
            return JsonSerializer.Deserialize<List<CreateEducationEntryRequest>>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? new();
        }
        catch (JsonException)
        {
            error = "Invalid education data format.";
            return null;
        }
    }
}


public class SubmissionFormRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Education { get; set; } = string.Empty;

    public IFormFile? File { get; set; }
}