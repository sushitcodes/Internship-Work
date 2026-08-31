namespace Form.Entities;

public class UserRoleAssignment
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public UserRole Role { get; set; }

    public User User { get; set; } = null!;
}