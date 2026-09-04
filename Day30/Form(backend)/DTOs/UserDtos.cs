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
public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string TemporaryPassword { get; set; } = string.Empty;

    // List, not a single role — matches your "multiple roles possible" decision.
    // Sent from the frontend as role NAMES ("Student","Staff","Admin"), not raw
    // ints, so a malformed request fails with a clear message instead of an
    // unexplained wrong-enum-value bug.
    public List<string> Roles { get; set; } = new();
}
public class CreatedUserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
}