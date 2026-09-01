namespace Form.DTOs;

public class EnrollmentDto
{
    public Guid Id { get; set; }
    public Guid StudentUserId { get; set; }
    public string StudentEmail { get; set; } = string.Empty;
    public Guid ClassRoomId { get; set; }
    public DateTime EnrolledAt { get; set; }
}

public class CreateEnrollmentRequest
{
    public Guid StudentUserId { get; set; }
    public Guid ClassRoomId { get; set; }
}