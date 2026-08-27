using Form.Entities;

namespace Form.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken> AddAsync(RefreshToken token);
    Task<RefreshToken?> GetByHashAsync(string tokenHash);
    Task SaveChangesAsync();
    Task RevokeAllForUserAsync(Guid userId);
}