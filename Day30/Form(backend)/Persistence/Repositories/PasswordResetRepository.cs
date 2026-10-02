using Form.Entities;
using Form.Interfaces;
using Form.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Form.Repositories;

public class PasswordResetRepository(AppDbContext _context) : IPasswordResetRepository
{
    public async Task<PasswordResetToken> AddAsync(PasswordResetToken token)
    {
        _context.PasswordResetTokens.Add(token);
        await _context.SaveChangesAsync();
        return token;
    }
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
    public async Task<PasswordResetToken?> GetLatestForUserAsync(Guid userId) =>
    await _context.PasswordResetTokens
        .Where(t => t.UserId == userId && !t.IsUsed)
        .OrderByDescending(t => t.CreatedAt)
        .FirstOrDefaultAsync();


}