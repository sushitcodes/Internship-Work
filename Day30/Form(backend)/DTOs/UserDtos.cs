namespace Form.DTOs;

public class UserProfileDto
{
    public Guid UserId { get; set; }

    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Address { get; set; } = string.Empty;

    public string? Gender { get; set; }

    public List<string> PhoneNumbers { get; set; } = new();

    public string? AvatarUrl { get; set; }

    public int MemberNumber { get; set; }
    public bool IsActive { get; set; }

}


public class UpdateOwnProfileRequest
{
    public string Address { get; set; } = string.Empty;

    public string? Gender { get; set; }

    public List<string> PhoneNumbers { get; set; } = new();

    public IFormFile? Avatar { get; set; }
}


public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;

    public string TemporaryPassword { get; set; } = string.Empty;

    // Multiple roles are allowed.
    // Frontend sends role names such as:
    // "Student", "Staff", "Admin"
    public List<string> Roles { get; set; } = new();
}


public class CreatedUserDto
{
    public Guid Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public List<string> Roles { get; set; } = new();
}
public class UpdateUserNameRequest
{
    public string FullName { get; set; } = string.Empty;
}

public class SetUserActiveRequest
{
    public bool IsActive { get; set; }
}