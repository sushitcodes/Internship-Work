namespace Form.DTOs
{
    public class CreateSubmissionRequest
    {
        public string FullName { get; set; } = string.Empty;
        public Guid ClassRoomId { get; set; }
        public int RollNo { get; set; }
        public Guid CreatedByUserId { get; set; }
        public IFormFile File { get; set; } = null!;
    }
}
