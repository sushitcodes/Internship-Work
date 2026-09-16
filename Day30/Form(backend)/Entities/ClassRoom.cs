namespace Form.Entities
{

    public class ClassRoom
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int AcademicYear { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid? ClassTeacherUserId { get; set; } //nullable because class can start without teacher
        public User? ClassTeacher { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}
