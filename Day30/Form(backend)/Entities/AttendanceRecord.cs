namespace Form.Entities
{
    public class AttendanceRecord
    {
        public Guid Id { get; set; }
        public Guid EnrollmentId { get; set; }
        public DateOnly Date { get; set; }
        public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
        public Guid MarkedByUserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Enrollment Enrollment { get; set; } = null!;
        public User MarkedByUser { get; set; } = null!;
    }
}
