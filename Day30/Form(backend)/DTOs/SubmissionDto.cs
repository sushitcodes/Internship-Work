namespace Form.DTOs
{
    public class ClassRoomDtos
    {
        public class SubmissionDto
        {
            public Guid Id { get; set; }
            public string FullName { get; set; } = string.Empty;
            public Guid ClassRoomId { get; set; }
            public string ClassRoomName { get; set; } = string.Empty;
            public int RollNo { get; set; }
            public string FileUrl { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
        }

        // Add CreateSubmissionRequest here
      

        // Add UpdateSubmissionRequest here
        
    }
}