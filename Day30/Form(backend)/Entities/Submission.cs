namespace Form.Entities;

public class Submission
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid? CreatedByUserId { get; set; }

    public string FullName { get; set; } = string.Empty;
    public Guid ClassRoomId { get; set; }
    public int RollNo { get; set; } // matches UserProfile.MemberNumber

    public string FileUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ClassRoom ClassRoom { get; set; } = null!;

    // Email, Phone, Education — GONE. EducationEntry.cs is now an orphaned
    // entity with nothing referencing it; don't delete the file yet (a
    // migration removing a whole table is riskier to reason about blind —
    // confirm nothing else uses it first), but know it's dead code now.
}