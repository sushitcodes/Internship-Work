namespace Form.DTOs
{
    public class UpdateSubmissionRequest
    {
        public string FullName { get; set; } = string.Empty;
        public Guid ClassRoomId { get; set; }
        public int RollNo { get; set; }
        public IFormFile? File { get; set; }
    }
}
