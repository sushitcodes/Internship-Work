using Form.Entities;
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

    private const int RotationGraceSeconds = 30;


    public async Task<RefreshRotationResult> ValidateAndRotateAsync(string rawToken)
    {
        var hash = Hash(rawToken);
        var existing = await _repository.GetByHashAsync(hash);
        var fail = new RefreshRotationResult(false, null, null, null);

        if (existing is null)
            return fail;

        if (existing.IsRevoked)
        {
            if (existing.ReplacedByTokenId is not null)
            {
                // A rotated token came back. If its replacement was issued seconds ago this is
                // almost certainly the same browser refreshing from two places, not a thief.
                var replacement = await _repository.GetByIdAsync(existing.ReplacedByTokenId.Value);
                var justRotated = replacement is not null
                    && replacement.CreatedAt > DateTime.UtcNow.AddSeconds(-RotationGraceSeconds);

                if (!justRotated)
                    await _repository.RevokeAllForUserAsync(existing.UserId);
            }
            return fail;
        }

        if (existing.ExpiresAt < DateTime.UtcNow)
            return fail;

        if (!existing.User.IsActive)
            return fail;

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
    public async Task RevokeAllForUserAsync(Guid userId)
    {
        await _repository.RevokeAllForUserAsync(userId);
    }
}