using Form.Domain.Entities;
using Form.DTOs;
using Form.Entities;
using Form.Hubs;
using Form.Interface;
using Form.Interfaces;
using Form.Persistence;
using Microsoft.AspNetCore.SignalR;

namespace Form.Services;

public class NotificationService(
    IHubContext<NotificationHub> hubContext,
    AppDbContext db,
    IUserRepository _userRepository,
    ILogger<NotificationService> logger
    ) : INotificationService
{
    public Task NotifyNewSubmissionAsync(
        IEnumerable<Guid> recipientUserIds,
        string studentName,
        string assignmentTitle,
        Guid submissionId,
        CancellationToken cancellationToken = default)
        => NotifyAsync(
            recipientUserIds,
            title: "New Submission Received",
            body: $"{studentName} uploaded a submission for \"{assignmentTitle}\".",
            link: $"/submission/{submissionId}",
            kind: "submission",
            cancellationToken);

    public Task NotifyGradePublishedAsync(
        Guid studentUserId,
        string subjectName,
        decimal marks,
        decimal maxMarks,
        CancellationToken cancellationToken = default)
    {
        var pct = maxMarks > 0 ? (marks / maxMarks) * 100 : 0;
        return NotifyAsync(
            new[] { studentUserId },
            title: "New Grade Published",
            body: $"Your grade for {subjectName} has been recorded: {marks}/{maxMarks} ({pct:0.#}%).",
            link: "/grades/report-card",
            kind: "grade",
            cancellationToken);
    }

    // Same message to many people.
    public Task NotifyAsync(
        IEnumerable<Guid> recipientUserIds,
        string title,
        string body,
        string? link = null,
        string kind = "generic",
        CancellationToken cancellationToken = default)
        => NotifyManyAsync(
            recipientUserIds.Distinct().Select(uid => new NotificationDraft(uid, title, body, link, kind)),
            cancellationToken);

    // One code path for everything: persist (ONE SaveChanges), then push.
    public async Task NotifyManyAsync(
        IEnumerable<NotificationDraft> drafts,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var entities = drafts.Select(d => new Notification
        {
            UserId = d.UserId,
            Title = d.Title,
            Body = d.Body,
            Link = d.Link,
            Kind = d.Kind,
            IsRead = false,
            CreatedAt = now,
        }).ToList();

        if (entities.Count == 0) return;

        db.Notifications.AddRange(entities);
        await db.SaveChangesAsync(cancellationToken);

        // Push in parallel chunks. A dead connection must never fail a request whose data is
        // already saved: the row exists, so the user sees it on their next fetch anyway.
        foreach (var chunk in entities.Chunk(50))
        {
            await Task.WhenAll(chunk.Select(async n =>
            {
                try
                {
                    var dto = new NotificationDto(n.Id, n.Title, n.Body, n.Link, n.Kind, n.IsRead, n.CreatedAt);
                    await hubContext.Clients.User(n.UserId.ToString())
                        .SendAsync("ReceiveNotification", dto, cancellationToken);
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "SignalR push failed for user {UserId}", n.UserId);
                }
            }));
        }
    }

    public Task NotifyAllTeachersAsync(
        string title, string body, string? link = null,
        CancellationToken cancellationToken = default)
        => NotifyAllByRolesAsync(new[] { UserRole.Staff, UserRole.Admin }, title, body, link, cancellationToken);

    public Task NotifyAllStudentsAsync(
        string title, string body, string? link = null,
        CancellationToken cancellationToken = default)
        => NotifyAllByRolesAsync(new[] { UserRole.Student }, title, body, link, cancellationToken);

    public Task NotifyEveryoneAsync(
        string title, string body, string? link = null,
        CancellationToken cancellationToken = default)
        => NotifyAllByRolesAsync(new[] { UserRole.Student, UserRole.Staff, UserRole.Admin }, title, body, link, cancellationToken);

    private async Task NotifyAllByRolesAsync(
        IEnumerable<UserRole> roles,
        string title, string body, string? link,
        CancellationToken cancellationToken)
    {
        var userIds = await _userRepository.GetUserIdsByRolesAsync(roles);
        if (userIds.Count == 0) return;

        await NotifyAsync(userIds, title, body, link, kind: "broadcast", cancellationToken);
    }
}