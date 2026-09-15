using System.Diagnostics;

namespace Form.Entities
{
    public class Grade : IAuditable
    {
        public Guid Id { get; set; }

        public Guid EnrollmentId{ get; set; }

        public Guid SubjectId { get; set; }

        public decimal MarksObtained { get; set; } // 0 start because decimal start with 0
        public decimal MaxMarks { get; set; } = 100;
        public string? Remarks { get; set; }

        public Guid GradedByUserId { get; set; }

        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? UpdatedAt { get; set; }

        public Enrollment Enrollment { get; set; } = null!;
        public Subject Subject { get; set; } = null!;
        public User GradedByUser { get; set; } = null!;
    }
}
