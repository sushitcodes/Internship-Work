namespace Form.Entities
{
    public class User
    {
        public Guid Id { get; set; }=
        Guid.NewGuid();
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<UserRoleAssignment> RoleAssignments { get; set; } = new List<UserRoleAssignment>();
        public UserProfile? Profile { get; set; }


    }
}
