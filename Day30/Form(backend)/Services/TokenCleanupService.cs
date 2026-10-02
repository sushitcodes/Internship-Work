using Form.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Form.Services;

public sealed class TokenCleanupService(IServiceScopeFactory scopes, ILogger<TokenCleanupService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromHours(6));
        do
        {
            try
            {
                using var scope = scopes.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var cutoff = DateTime.UtcNow.AddDays(-7);

                var tokens = await db.RefreshTokens
                    .Where(t => t.ExpiresAt < cutoff || (t.IsRevoked && t.CreatedAt < cutoff))
                    .ExecuteDeleteAsync(stoppingToken);

                var resets = await db.PasswordResetTokens
                    .Where(t => t.ExpiresAt < cutoff)
                    .ExecuteDeleteAsync(stoppingToken);

                var notes = await db.Notifications
                    .Where(n => n.IsRead && n.CreatedAt < DateTime.UtcNow.AddDays(-90))
                    .ExecuteDeleteAsync(stoppingToken);

                logger.LogInformation(
                    "Cleanup removed {Tokens} refresh tokens, {Resets} reset codes, {Notes} old notifications",
                    tokens, resets, notes);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Token cleanup failed");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}