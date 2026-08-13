namespace Project1_backend_.DTOs
{
    public class SubmissionDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public List<EducationDto> Education { get; set; } = new();
    }

    public class SubmissionResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public List<EducationResponseDto> Education { get; set; } = new();
    }
    public class EducationResponseDto
    {
        public int Id { get; set; }
        public string Degree { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty;
        public string School { get; set; } = string.Empty;
    }
}

