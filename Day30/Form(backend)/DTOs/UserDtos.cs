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
    [System.ComponentModel.DataAnnotations.StringLength(300)]
    public string Address { get; set; } = string.Empty;

    [System.ComponentModel.DataAnnotations.MaxLength(5)]
    public List<string> PhoneNumbers { get; set; } = new();
    public string? Gender { get; set; }


    public IFormFile? Avatar { get; set; }
}


public class CreateUserRequest
{
    [System.ComponentModel.DataAnnotations.Required,
     System.ComponentModel.DataAnnotations.EmailAddress,
     System.ComponentModel.DataAnnotations.StringLength(256)]
    public string Email { get; set; } = string.Empty;

    // Optional. If empty, the part before "@" is used (same rule as before).
    [System.ComponentModel.DataAnnotations.StringLength(100)]
    public string? FullName { get; set; }

    // 72: BCrypt ignores everything past 72 bytes.
    [System.ComponentModel.DataAnnotations.Required,
     System.ComponentModel.DataAnnotations.StringLength(72, MinimumLength = 8)]
    public string TemporaryPassword { get; set; } = string.Empty;

    // Frontend sends role names: "Student", "Staff", "Admin"
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