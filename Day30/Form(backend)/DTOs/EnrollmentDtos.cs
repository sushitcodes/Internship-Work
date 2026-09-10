namespace Form.DTOs;

public class EnrollmentDto
{
    public Guid Id { get; set; }
    public Guid StudentUserId { get; set; }
    public string StudentEmail { get; set; } = string.Empty;
    public string StudentFullName { get; set; }=string.Empty;
    public int RollNo { get; set; }
    public Guid ClassRoomId { get; set; }
    public string ClassRoomName { get; set; } =string.Empty;
    public DateTime EnrolledAt { get; set; }
}

public class CreateEnrollmentRequest
{
    public Guid StudentUserId { get; set; }
    public Guid ClassRoomId { get; set; }
}