namespace Form.DTOs
{
    public class UpdateSubmissionRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public List<CreateEducationEntryRequest> Education { get; set; } = new();
        public IFormFile? File { get; set; }

    }
}
