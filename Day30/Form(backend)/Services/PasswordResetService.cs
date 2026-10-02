using Form.Entities;
using Form.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace Form.Services;

public class PasswordResetService : IPasswordResetService
{
    private const int ExpiryMinutes = 10;
    private const int MaxAttempts = 5;
    // the ONE real source of truth for this number

    private readonly IPasswordResetRepository _repository;

    public PasswordResetService(IPasswordResetRepository repository)
    {
        _repository = repository;
    }

    public async Task<(string rawToken, int expiryMinutes)> GenerateAsync(Guid userId)
    {
        var rawToken = GenerateSixDigitCode();

        var entity = new PasswordResetToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = Hash(rawToken),
            ExpiresAt = DateTime.UtcNow.AddMinutes(ExpiryMinutes),
        };

        await _repository.AddAsync(entity);
        return (rawToken, ExpiryMinutes);
    }

    //private static string GenerateSecureRandomToken()
    //{
    //    var bytes = RandomNumberGenerator.GetBytes(32);
    //    return Convert.ToBase64String(bytes)
    //        .Replace("+", "-").Replace("/", "_").Replace("=", "");  
    //}
    private static string GenerateSixDigitCode()
    {
        var value = RandomNumberGenerator.GetInt32(0, 1_000_000);
        return value.ToString("D6");   // always 6 digits, zero-padded (e.g. "003417")
    }

    private static string Hash(string code)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(code));
        return Convert.ToBase64String(bytes);
    }

    public async Task<bool> ValidateAsync(Guid userId, string code)
    {
        var existing = await _repository.GetLatestForUserAsync(userId);

        if (existing is null) return false;
        if (existing.ExpiresAt < DateTime.UtcNow) return false;

        // Too many wrong guesses: burn this code. The user must request a new one.
        if (existing.FailedAttempts >= MaxAttempts)
        {
            existing.IsUsed = true;
            await _repository.SaveChangesAsync();
            return false;
        }

        var supplied = Encoding.UTF8.GetBytes(Hash(code));
        var stored = Encoding.UTF8.GetBytes(existing.TokenHash);
        if (!CryptographicOperations.FixedTimeEquals(supplied, stored))
        {
            existing.FailedAttempts++;
            await _repository.SaveChangesAsync();
            return false;
        }

        existing.IsUsed = true;   // one time use
        await _repository.SaveChangesAsync();
        return true;
    }
}