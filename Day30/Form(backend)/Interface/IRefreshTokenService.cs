using Form.Entities;

namespace Form.Interface
{
    public record RefreshRotationResult(
    bool Success,
    User? User,
    string? NewRawToken,
    DateTime? NewExpiresAt
);
    public interface IRefreshTokenService
    {
        Task<(string rawToken, DateTime expiresAt)> GenerateAsync(Guid userid);
        Task<RefreshRotationResult> ValidateAndRotateAsync(string rawToken);
        Task RevokeAsync(string rawToken);
        Task RevokeAllForUserAsync(Guid userId);

    }
}
