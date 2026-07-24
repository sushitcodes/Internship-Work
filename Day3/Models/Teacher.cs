namespace SchoolManagementSystem.Models
{
    public class Teacher : Person
    {
        public string Subject { get; set; }

        public Teacher(int id, string name, int age, string subject)
            : base(id, name, age)
        {
            Subject = subject;
        }

        // Same abstract method, completely different implementation —
        // this is polymorphism: a List<Person> can hold Students AND Teachers,
        // and calling GetSummary() on each runs the RIGHT version automatically.
        public override string GetSummary() => $"Teacher: {Name}, teaches {Subject}";

        public override string ToString() => GetSummary();
    }
}
