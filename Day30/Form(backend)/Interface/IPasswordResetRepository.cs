using Form.Entities;

namespace Form.Interfaces;

public interface IPasswordResetRepository
{
    Task<PasswordResetToken> AddAsync(PasswordResetToken token);
    Task<PasswordResetToken?> GetLatestForUserAsync(Guid userId);
    Task SaveChangesAsync();
}