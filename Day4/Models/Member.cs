namespace LibraryManagementSystem.Models
{
    public class Member
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"[{Id}] {Name} ({Email})";
        }
    }
}
