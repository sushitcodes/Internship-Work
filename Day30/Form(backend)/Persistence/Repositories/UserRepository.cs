using Form.Entities;
using Form.Interface;
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
}