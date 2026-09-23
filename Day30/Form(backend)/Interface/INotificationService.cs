namespace Form.Interface;

public interface INotificationService
{
    Task NotifyNewSubmissionAsync(
            IEnumerable<Guid> recipientUserIds,
            string studentName,
            string assignmentTitle,
            Guid submissionId,
            CancellationToken cancellationToken = default);
    Task NotifyGradePublishedAsync(
            Guid studentUserId,
            string subjectName,
            decimal marks,
            decimal maxMarks,
            CancellationToken cancellationToken = default);
    Task NotifyAsync(
      IEnumerable<Guid> recipientUserIds,
      string title,
      string body,
      string? link = null,
      string kind = "generic",
      CancellationToken cancellationToken = default);


    Task NotifyAllTeachersAsync(
    string title,
    string body,
    string? link = null,
    CancellationToken cancellationToken = default);

    Task NotifyAllStudentsAsync(
        string title,
        string body,
        string? link = null,
        CancellationToken cancellationToken = default);

    Task NotifyEveryoneAsync(
        string title,
        string body,
        string? link = null,
        CancellationToken cancellationToken = default);
}
