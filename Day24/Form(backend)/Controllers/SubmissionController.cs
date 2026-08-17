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
    [RequestSizeLimit(10 * 1024 * 1024)] // hard cap at the ASP.NET pipeline level
    public async Task<ActionResult<SubmissionDto>> Create([FromForm] SubmissionFormRequest form)
    {
        if (form.File is null || form.File.Length == 0)
            return BadRequest("A file is required.");

        // The frontend sends "education" as a JSON string inside FormData —
        // model binding can't auto-parse nested JSON from a form field,
        // so we deserialize it manually here.
        List<CreateEducationEntryRequest> education;
        try
        {
            education = JsonSerializer.Deserialize<List<CreateEducationEntryRequest>>(
                form.Education,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            ) ?? new();
        }
        catch (JsonException)
        {
            return BadRequest("Invalid education data format.");
        }

        var request = new CreateSubmissionRequest
        {
            FullName = form.FullName,
            Email = form.Email,
            Phone = form.Phone,
            Education = education,
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
    public async Task<ActionResult<List<SubmissionDto>>> GetAll()
        => Ok(await _submissionService.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SubmissionDto>> GetById(Guid id)
    {
        var result = await _submissionService.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }
}

// Field names here must match exactly what the frontend appends to FormData
// (formData.append("fullName", ...), etc.) — case-insensitive but not
// tolerant of different wording.
public class SubmissionFormRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Education { get; set; } = string.Empty;
    public IFormFile File { get; set; } = default!;
}
