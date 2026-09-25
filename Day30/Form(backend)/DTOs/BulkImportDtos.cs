namespace Form.DTOs;
public class BulkImportResultDto
{
    public bool Success { get; set; }
    public int ImportedCount { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class ParsedStudentRow
{
    public int RowNumber { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string TemporaryPassword { get; set; } = string.Empty;
}