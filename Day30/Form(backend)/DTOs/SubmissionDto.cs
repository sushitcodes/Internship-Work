namespace Form.Dtos

{
    public class Class
    {
    public class EducationEntryDto
    {
        public string Institution { get; set; } = string.Empty;
        public string Degree { get; set; } = string.Empty;
        public int Year { get; set; }
    }

    public class SubmissionDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<EducationEntryDto> Education { get; set; } = new();
    }
}
}
