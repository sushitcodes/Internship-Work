using Form.Entities;
using Form.Interfaces;
using Form.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Form.Repositories;

public class RefreshTokenRepository(AppDbContext _context) : IRefreshTokenRepository
{
    public async Task<RefreshToken> AddAsync(RefreshToken token)
    {
        _context.RefreshTokens.Add(token);
        await _context.SaveChangesAsync();
        return token;
    }

    public async Task<RefreshToken?> GetByHashAsync(string tokenHash) =>
        await _context.RefreshTokens
            .Include(t => t.User)
                    .ThenInclude(u => u.RoleAssignments)

            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

    public async Task<RefreshToken?> GetByIdAsync(Guid id) =>
    await _context.RefreshTokens.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);

    // One UPDATE statement, no rows loaded into memory.
    public async Task RevokeAllForUserAsync(Guid userId) =>
        await _context.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.IsRevoked, true));
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}