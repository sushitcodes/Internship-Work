using Microsoft.AspNetCore.Http;

namespace Form.DTOs;

public class CreateEducationEntryRequest
{
    public string Institution { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public int Year { get; set; }
}

// Internal request shape used inside the Application layer — decoupled
// from HTTP/multipart-form concerns, which live in the API layer.
public class CreateSubmissionRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public List<CreateEducationEntryRequest> Education { get; set; } = new();
    public IFormFile File { get; set; } = default!;
}
