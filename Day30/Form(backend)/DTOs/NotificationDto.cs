namespace Form.DTOs;

public record NotificationDto(
    Guid Id,
    string Title,
    string Body,
    string? Link,
    string Kind,
    bool IsRead,
    DateTime  CreatedAt);

public record BroadcastRequest(
string Title,
string Body,
string? Link,
String Scope);

public enum BroadcastScope
{
    AllTeachers,
    AllStudents,
    Everyone,
}
