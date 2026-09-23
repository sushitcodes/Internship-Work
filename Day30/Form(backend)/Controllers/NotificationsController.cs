using Form.DTOs;
using Form.Interface;
using Form.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Form.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize]
public class NotificationsController(
    AppDbContext db,
        INotificationService _notificationService

    ) : ControllerBase
{
    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirst("sub")?.Value
                ?? User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

    /// <summary>List the current user's notifications, newest first.</summary>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<NotificationDto>>> GetMine(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var query = db.Notifications
            .Where(n => n.UserId == CurrentUserId)
            .OrderByDescending(n => n.CreatedAt)
            .AsQueryable();

        if (unreadOnly) query = query.Where(n => !n.IsRead);

        var result = await query
            .Take(Math.Clamp(limit, 1, 200))
            .Select(n => new NotificationDto(
                n.Id, n.Title, n.Body, n.Link, n.Kind, n.IsRead, n.CreatedAt))
            .ToListAsync(cancellationToken);

        return Ok(result);
    }

    /// <summary>Cheap endpoint for the bell badge.</summary>
    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount(
        CancellationToken cancellationToken = default)
    {
        var count = await db.Notifications
            .CountAsync(n => n.UserId == CurrentUserId && !n.IsRead, cancellationToken);
        return Ok(count);
    }

    /// <summary>Mark a single notification as read.</summary>
    [HttpPatch("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var notification = await db.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == CurrentUserId,
                cancellationToken);

        if (notification is null) return NotFound();

        notification.IsRead = true;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    // every unread notification for the current user as read.</summary>
    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken = default)
    {
        await db.Notifications
            .Where(n => n.UserId == CurrentUserId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true),
                cancellationToken);
        return NoContent();

    }
    public record BroadcastRequest(
string Title,
string Body,
string? Link,
BroadcastScope Scope);

    public enum BroadcastScope
    {
        AllTeachers,
        AllStudents,
        Everyone,
    }

    [HttpPost("broadcast")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> Broadcast(
        [FromBody] BroadcastRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest("Title is required.");

        if (string.IsNullOrWhiteSpace(request.Body))
            return BadRequest("Body is required.");

        // Cap the length so a runaway script can't insert a megabyte per row.
        if (request.Title.Length > 200 || request.Body.Length > 1000)
            return BadRequest("Title max 200 chars, body max 1000 chars.");

        switch (request.Scope)
        {
            case BroadcastScope.AllTeachers:
                await _notificationService.NotifyAllTeachersAsync(
                    request.Title, request.Body, request.Link, cancellationToken);
                break;

            case BroadcastScope.AllStudents:
                await _notificationService.NotifyAllStudentsAsync(
                    request.Title, request.Body, request.Link, cancellationToken);
                break;

            case BroadcastScope.Everyone:
                await _notificationService.NotifyEveryoneAsync(
                    request.Title, request.Body, request.Link, cancellationToken);
                break;

            default:
                return BadRequest("Unknown scope.");
        }

        return NoContent();
    }
}