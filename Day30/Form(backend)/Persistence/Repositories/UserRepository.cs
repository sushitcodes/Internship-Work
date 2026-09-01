using Form.Entities;
using Form.Interfaces;
using Form.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Form.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;
    public UserRepository(AppDbContext context) => _context = context;
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
    await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
    public async Task<List<User>> GetByRoleAsync(UserRole role) =>
    await _context.Users
        .Where(u => u.RoleAssignments.Any(ra => ra.Role == role))
        .ToListAsync();
}