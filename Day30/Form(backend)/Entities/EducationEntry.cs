namespace Form.Entities;

public class EducationEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Institution { get; set; } = string.Empty;
    public string Degree { get; set; } = string.Empty;
    public int Year { get; set; }

    public Guid SubmissionId { get; set; }
    public Submission? Submission { get; set; }
}
