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
    public async Task<UserProfile?> GetByMemberNumberAsync(int memberNumber) =>
    await _context.UserProfiles.FirstOrDefaultAsync(p => p.MemberNumber == memberNumber);
    public async Task<(List<UserProfile> Items, int TotalCount)> SearchAsync(int page, int pageSize, string? search)
    {
        // Query FROM Users, not UserProfiles. A user who registered (or was
        // Admin-created) but never opened their own profile page has no
        // UserProfile row yet — that's expected, lazy-creation is intentional.
        // But they still need to be VISIBLE in this list, so we start from the
        // table that's guaranteed to have a row for every real account.
        var query =
            from u in _context.Users
            join p in _context.UserProfiles on u.Id equals p.UserId into profileJoin
            from p in profileJoin.DefaultIfEmpty() // LEFT JOIN — p is null when no profile exists yet
            select new { User = u, Profile = p };

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(x =>
                x.User.Email.Contains(search) ||
                (x.Profile != null && x.Profile.FullName.Contains(search)));
        }

        var totalCount = await query.CountAsync();

        var raw = await query
            // Users with a real profile sort by their assigned number;
            // users without one yet sort last (int.MaxValue), not first.
            .OrderBy(x => x.Profile != null ? x.Profile.MemberNumber : int.MaxValue)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Where no profile exists, build a placeholder in memory — NOT saved to
        // the database. MemberNumber = 0 is a sentinel the frontend can check
        // for ("no profile yet") rather than a real assigned number.
        var items = raw.Select(x => x.Profile ?? new UserProfile
        {
            UserId = x.User.Id,
            User = x.User,
            FullName = x.User.Email,
            Phone = string.Empty,
            MemberNumber = 0,
        }).ToList();


        return (items, totalCount);
    }
}