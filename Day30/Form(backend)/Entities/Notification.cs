namespace Form.Domain.Entities;
public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>The user this notification belongs to.</summary>
    public Guid UserId { get; set; }

    public string Title { get; set; } = default!;
    public string Body { get; set; } = default!;

    /// <summary>Optional deep-link path, e.g. "/submissions/{id}".</summary>
    public string? Link { get; set; }

    /// <summary>Discriminator for UI styling: "submission", "grade", "generic".</summary>
    public string Kind { get; set; } = "generic";

    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}