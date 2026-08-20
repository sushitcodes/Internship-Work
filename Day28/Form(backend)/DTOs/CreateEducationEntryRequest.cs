namespace Form.DTOs
{
    public class CreateEducationEntryRequest
    {
        public string Institution { get; set; } = string.Empty;
        public string Degree { get; set; } = string.Empty;
        public int Year { get; set; }
    }
}
