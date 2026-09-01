namespace Form.Entities;

public class Enrollment
{
    public Guid Id { get; set; }
    public Guid StudentUserId { get; set; }
    public Guid ClassRoomId { get; set; }
    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

    public User StudentUser { get; set; } = null!;
    public ClassRoom ClassRoom { get; set; } = null!;
    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
}