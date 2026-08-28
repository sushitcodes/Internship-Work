using Form.Entities;
using Form.Interface;
using Form.Interfaces;
using System.Security.Cryptography;

namespace Form.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly IRefreshTokenRepository _repository;

    public RefreshTokenService(IRefreshTokenRepository repository)
    {
        _repository = repository;
    }

    public async Task<(string rawToken, DateTime expiresAt)> GenerateAsync(Guid userId)
    {
        var (entity, rawToken) = await CreateTokenEntityAsync(userId);
        return (rawToken, entity.ExpiresAt);
    }

    public async Task<RefreshRotationResult> ValidateAndRotateAsync(string rawToken)
    {
        var hash = Hash(rawToken);
        var existing = await _repository.GetByHashAsync(hash);

        // Unknown token — never issued, or already deleted. Reject.
        if (existing is null)
            return new RefreshRotationResult(false, null, null, null);

        if (existing.IsRevoked)
        {
            // Revoked can mean two very different things:
            //  1. Normal logout — ReplacedByTokenId is still null.
            //  2. This token was already rotated once (has a replacement)
            //     and is now being presented AGAIN. That only happens if
            //     someone captured an old token — the real user already
            //     moved on to its replacement. Treat this as theft: kill
            //     every active session this user has, not just this token.
            if (existing.ReplacedByTokenId is not null)
            {
                await _repository.RevokeAllForUserAsync(existing.UserId);
            }
            return new RefreshRotationResult(false, null, null, null);
        }

        if (existing.ExpiresAt < DateTime.UtcNow)
            return new RefreshRotationResult(false, null, null, null);

        // Valid — rotate. Issue a fresh token, then revoke this one and
        // link it forward, so any future reuse of THIS raw token is
        // recognizable as theft (branch above).
        var (newEntity, newRawToken) = await CreateTokenEntityAsync(existing.UserId);

        existing.IsRevoked = true;
        existing.ReplacedByTokenId = newEntity.Id;
        await _repository.SaveChangesAsync();

        return new RefreshRotationResult(true, existing.User, newRawToken, newEntity.ExpiresAt);
    }

    private async Task<(RefreshToken entity, string rawToken)> CreateTokenEntityAsync(Guid userId)
    {
        var rawToken = GenerateSecureRandomToken();
        var entity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = Hash(rawToken),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
        };

        await _repository.AddAsync(entity);
        return (entity, rawToken);
    }

    private static string GenerateSecureRandomToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    private static string Hash(string rawToken)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToBase64String(bytes);
    }
    public async Task RevokeAsync(string rawToken)
    {
        var hash = Hash(rawToken);
        var existing = await _repository.GetByHashAsync(hash);

        // Already gone, already revoked, or was never real — nothing to do.
        // Logout should never fail just because the token was already invalid.
        if (existing is null || existing.IsRevoked)
            return;

        existing.IsRevoked = true;
        await _repository.SaveChangesAsync();
    }
}