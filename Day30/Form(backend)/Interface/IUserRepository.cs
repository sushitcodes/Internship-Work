using Form.Entities;


namespace Form.Interfaces
{
    public interface IUserRepository
    {
        Task<int> CountByRoleAsync(UserRole role);
        Task<User?> GetByEmailAsync(string email);
        Task<User> AddAsync(User user);
        Task UpdateAsync(User user);
        Task<User?> GetByIdAsync(Guid id);
        Task<List<User>> GetByRoleAsync(UserRole role);
    }
}
