using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace Form.Controllers;

[ApiController]
[Authorize]   // must be logged in
public class FilesController(IWebHostEnvironment env) : ControllerBase
{
    private static readonly FileExtensionContentTypeProvider Types = new();

    // Same URL shape the database already stores: /uploads/<guid>.<ext>
    [HttpGet("/uploads/{name}")]
    public IActionResult Get(string name)
    {
        var safeName = Path.GetFileName(name);   // strips "../" tricks
        var path = Path.Combine(env.ContentRootPath, "App_Data", "uploads", safeName);
        if (!System.IO.File.Exists(path)) return NotFound();

        if (!Types.TryGetContentType(safeName, out var contentType))
            contentType = "application/octet-stream";

        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return PhysicalFile(path, contentType);
    }
}