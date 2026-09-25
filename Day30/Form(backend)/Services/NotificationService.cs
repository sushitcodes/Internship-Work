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
    IUserRepository _userRepository
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
            link: "/grades",
            kind: "grade",
            cancellationToken);
    }

    /// One code path for all notifications: persist, then push.
    /// Persistence and delivery stay in sync — no way to push something
    /// that wasn't saved, no way to save something that wasn't pushed.
    public async Task NotifyAsync(
        IEnumerable<Guid> recipientUserIds,
        string title,
        string body,
        string? link = null,
        string kind = "generic",
        CancellationToken cancellationToken = default)
    {
        var recipients = recipientUserIds.Distinct().ToList();
        if (recipients.Count == 0) return;

        var now = DateTime.UtcNow;

        // Persist one row per recipient (read-state is per-user)
        var entities = recipients.Select(uid => new Notification
        {
            UserId = uid,
            Title = title,
            Body = body,
            Link = link,
            Kind = kind,
            IsRead = false,
            CreatedAt = now,
        }).ToList();

        db.Notifications.AddRange(entities);
        await db.SaveChangesAsync(cancellationToken);

        //  Push over SignalR. Clients.User(userId) resolves to all
        //    active connections for that user (multiple tabs = multiple).
        foreach (var entity in entities)
        {
            var dto = new NotificationDto(
                entity.Id,
                entity.Title,
                entity.Body,
                entity.Link,
                entity.Kind,
                entity.IsRead,
                entity.CreatedAt);

            await hubContext.Clients
                .User(entity.UserId.ToString())
                .SendAsync("ReceiveNotification", dto, cancellationToken);
        }
    }

    public Task NotifyAllTeachersAsync(
    string title, string body, string? link = null,
    CancellationToken cancellationToken = default)
    => NotifyAllByRolesAsync(
        new[] { UserRole.Staff, UserRole.Admin },
        title, body, link, cancellationToken);

    public Task NotifyAllStudentsAsync(
        string title, string body, string? link = null,
        CancellationToken cancellationToken = default)
        => NotifyAllByRolesAsync(
            new[] { UserRole.Student },
            title, body, link, cancellationToken);

    public Task NotifyEveryoneAsync(
        string title, string body, string? link = null,
        CancellationToken cancellationToken = default)
        => NotifyAllByRolesAsync(
            new[] { UserRole.Student, UserRole.Staff, UserRole.Admin },
            title, body, link, cancellationToken);

    private async Task NotifyAllByRolesAsync(
        IEnumerable<UserRole> roles,
        string title, string body, string? link,
        CancellationToken cancellationToken)
    {
        var userIds = await _userRepository.GetUserIdsByRolesAsync(roles);
        if (userIds.Count == 0) return;

        await NotifyAsync(
            userIds,
            title,
            body,
            link,
            kind: "broadcast",
            cancellationToken);
    }
}