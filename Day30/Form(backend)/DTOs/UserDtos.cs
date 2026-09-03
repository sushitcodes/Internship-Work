namespace Form.DTOs;
public class UserProfileDto
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public int MemberNumber { get; set; } // display-only, e.g. "Member #43"
}

public class UpdateOwnProfileRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public IFormFile? Avatar { get; set; }
}