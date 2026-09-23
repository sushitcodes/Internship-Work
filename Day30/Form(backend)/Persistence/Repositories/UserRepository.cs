using Form.Entities;
using Form.Interface;
using Form.Interfaces;
using Form.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Form.Repositories;

public class UserRepository(AppDbContext _context) : IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email) =>
        await _context.Users
            .Include(u => u.RoleAssignments)
            .FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User> AddAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }
    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
    public async Task<User?> GetByIdAsync(Guid id) =>
    await _context.Users.Include(u => u.RoleAssignments).FirstOrDefaultAsync(u => u.Id == id);
    public async Task<List<User>> GetByRoleAsync(UserRole role) =>
    await _context.Users
        .Where(u => u.RoleAssignments.Any(ra => ra.Role == role))
        .ToListAsync();
    // COUNT, not GetByRoleAsync().Count — the difference matters: this becomes
    // a single SQL COUNT(*) query, never pulling full User rows (with their
    // RoleAssignments collections) into memory just to count them.
    public async Task<int> CountByRoleAsync(UserRole role) =>
        await _context.Users.CountAsync(u => u.RoleAssignments.Any(ra => ra.Role == role));
    public async Task SetActiveStatusAsync(Guid userId, bool isActive)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user is null) return;
        user.IsActive = isActive;
        await _context.SaveChangesAsync();
    }
    public async Task<List<Guid>> GetUserIdsByRoleAsync(UserRole role) =>
    await _context.Users
        .AsNoTracking()
        .Where(u => u.IsActive &&
                    u.RoleAssignments.Any(ra => ra.Role == role))
        .Select(u => u.Id)
        .ToListAsync();

    public async Task<List<Guid>> GetUserIdsByRolesAsync(IEnumerable<UserRole> roles)
    {
        var set = roles.Distinct().ToList();
        return await _context.Users
            .AsNoTracking()
            .Where(u => u.IsActive &&
                        u.RoleAssignments.Any(ra => set.Contains(ra.Role)))
            .Select(u => u.Id)
            .ToListAsync();
    }

    public async Task<List<Guid>> GetAllUserIdsAsync() =>
        await _context.Users
            .AsNoTracking()
            .Where(u => u.IsActive)
            .Select(u => u.Id)
            .ToListAsync();
}