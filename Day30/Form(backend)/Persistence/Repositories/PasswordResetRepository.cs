using Form.Entities;
using Form.Interfaces;
using Form.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Form.Repositories;

public class PasswordResetRepository : IPasswordResetRepository
{
    private readonly AppDbContext _context;
    public PasswordResetRepository(AppDbContext context) => _context = context;

    public async Task<PasswordResetToken> AddAsync(PasswordResetToken token)
    {
        _context.PasswordResetTokens.Add(token);
        await _context.SaveChangesAsync();
        return token;
    }

    public async Task<PasswordResetToken?> GetByHashAsync(string tokenHash) =>
        await _context.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    public async Task<PasswordResetToken?> GetLatestForUserAsync(Guid userId) =>
    await _context.PasswordResetTokens
        .Where(t => t.UserId == userId && !t.IsUsed)
        .OrderByDescending(t => t.CreatedAt)
        .FirstOrDefaultAsync();


}