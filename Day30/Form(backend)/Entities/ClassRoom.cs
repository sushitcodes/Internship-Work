namespace Form.Entities
{
    public class ClassRoom : ISoftDelete
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int AcademicYear { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Soft delete. Implementing ISoftDelete also means an accidental _context.Remove(classRoom)
        // becomes an UPDATE (your SaveChanges override) and can NOT cascade-delete enrollments and subjects.
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }

        public Guid? ClassTeacherUserId { get; set; }
        public User? ClassTeacher { get; set; }
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    }
}