namespace Form.Entities;

public class UserProfile
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string FullName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public Gender? Gender { get; set; }
    public List<string> PhoneNumbers { get; set; } = new();
    public string? AvatarUrl { get; set; }

    // No setter logic needed here — the database assigns this automatically
    // on insert, starting at 1 and counting up. We never set this in C# code.
    public int MemberNumber { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}