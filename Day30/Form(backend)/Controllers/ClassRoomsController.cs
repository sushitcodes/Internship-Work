using Form.DTOs;
using Form.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace Form.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
//Primary Constructor
public class ClassRoomsController(IClassRoomService classRoomService) : ControllerBase
{ 
    //base constructor
//public class ClassRoomsController : ControllerBase

//    private readonly IClassRoomService _classRoomService;
//    public ClassRoomsController(IClassRoomService classRoomService) => _classRoomService = classRoomService;

    [HttpGet]
    [Authorize(Policy = "StaffOrAdmin")]
    public async Task<ActionResult<List<ClassRoomDto>>> GetAll() =>
        Ok(await classRoomService.GetAllAsync());

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ClassRoomDto>> Create(CreateClassRoomRequest request) =>
        Ok(await classRoomService.CreateAsync(request));

    [HttpPut("{id:guid}/class-teacher")]
    [Authorize(Policy = "AdminOnly")]  // appointing a head teacher is an admin decision, not a teacher's own
    public async Task<IActionResult> AssignClassTeacher(Guid id, AssignClassTeacherRequest request)
    {
        try
        {
            await classRoomService.AssignClassTeacherAsync(id, request.TeacherUserId);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}