using System.Threading.Channels;
using Form.Interfaces;

namespace Form.Services;

public sealed record EmailJob(string To, string Code, int ExpiryMinutes);

public sealed class EmailQueue
{
    private readonly Channel<EmailJob> _channel = Channel.CreateUnbounded<EmailJob>();
    public void Enqueue(EmailJob job) => _channel.Writer.TryWrite(job);
    public IAsyncEnumerable<EmailJob> ReadAllAsync(CancellationToken ct) => _channel.Reader.ReadAllAsync(ct);
}

// Sends queued emails one by one in the background. A failed send is logged, never shown to the requester.
public sealed class EmailSenderWorker(
    EmailQueue queue, IServiceScopeFactory scopes, ILogger<EmailSenderWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var job in queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = scopes.CreateScope();
                var email = scope.ServiceProvider.GetRequiredService<IEmailService>();
                await email.SendPasswordResetCodeAsync(job.To, job.Code, job.ExpiryMinutes);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send password reset email to {Email}", job.To);
            }
        }
    }
}