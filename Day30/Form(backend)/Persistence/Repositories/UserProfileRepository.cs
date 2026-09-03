using Form.Entities;
using Form.Interfaces;
using Form.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Form.Repositories;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly AppDbContext _context;

    public UserProfileRepository(AppDbContext context) => _context = context;

    public async Task<UserProfile?> GetByUserIdAsync(Guid userId)
    {
        // .Include(p => p.User) — we need this because UserProfileDto shows
        // Email, which lives on User, not UserProfile. Without this, p.User
        // would be null and mapping would throw.
        return await _context.UserProfiles
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    public async Task<UserProfile> AddAsync(UserProfile profile)
    {
        _context.UserProfiles.Add(profile);
        await _context.SaveChangesAsync();

        // Reload with .Include so the returned object has User populated —
        // straight after Add, EF hasn't fetched the related User row yet.
        return await GetByUserIdAsync(profile.UserId) ?? profile;
    }

    public async Task<UserProfile?> UpdateAsync(Guid userId, UserProfile updated)
    {
        var existing = await _context.UserProfiles
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (existing is null) return null;

        existing.FullName = updated.FullName;
        existing.Phone = updated.Phone;

        // Same "empty string means no new file" convention as SubmissionRepository.UpdateAsync —
        // keeping this consistent rather than inventing a different convention here.
        if (!string.IsNullOrEmpty(updated.AvatarUrl))
            existing.AvatarUrl = updated.AvatarUrl;

        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<(List<UserProfile> Items, int TotalCount)> SearchAsync(int page, int pageSize, string? search)
    {
        var query = _context.UserProfiles.Include(p => p.User).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p =>
                p.FullName.Contains(search) || p.User.Email.Contains(search));
        }

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(p => p.MemberNumber)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }
}