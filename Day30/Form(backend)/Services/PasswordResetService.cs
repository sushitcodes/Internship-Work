// Form.Services/PasswordResetService.cs
using System.Security.Cryptography;
using Form.Entities;
using Form.Interfaces;

namespace Form.Services;

public class PasswordResetService : IPasswordResetService
{
    private const int ExpiryMinutes = 2;  // the ONE real source of truth for this number

    private readonly IPasswordResetRepository _repository;

    public PasswordResetService(IPasswordResetRepository repository)
    {
        _repository = repository;
    }

    public async Task<(string rawToken, int expiryMinutes)> GenerateAsync(Guid userId)
    {
        var rawToken = GenerateSecureRandomToken();

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

    private static string GenerateSecureRandomToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-").Replace("/", "_").Replace("=", "");  
    }

    private static string Hash(string rawToken)
    {
        var bytes = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawToken));
        return Convert.ToBase64String(bytes);
    }
}