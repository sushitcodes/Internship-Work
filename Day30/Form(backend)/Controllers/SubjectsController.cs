using Form.DTOs;
using Form.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Form.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SubjectsController : ControllerBase
{
    private readonly ISubjectService _subjectService;
    public SubjectsController(ISubjectService subjectService) => _subjectService = subjectService;

    [HttpGet("class/{classRoomId:guid}")]
    public async Task<ActionResult<List<SubjectDto>>> GetByClassRoom(Guid classRoomId) =>
        Ok(await _subjectService.GetByClassRoomAsync(classRoomId));

    [HttpPost]
    [Authorize(Policy = "StaffOrAdmin")]
    public async Task<ActionResult<SubjectDto>> Create(CreateSubjectRequest request) =>
        Ok(await _subjectService.CreateAsync(request));

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "StaffOrAdmin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _subjectService.ArchiveAsync(id);   // ← was DeleteAsync
        return NoContent();
    }
}