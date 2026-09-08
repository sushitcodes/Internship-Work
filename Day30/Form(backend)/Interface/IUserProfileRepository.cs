using Form.Entities;

namespace Form.Interfaces;

public interface IUserProfileRepository
{
    Task<UserProfile?> GetByUserIdAsync(Guid userId);
    Task<UserProfile> AddAsync(UserProfile profile);
    Task<UserProfile?> UpdateAsync(Guid userId, UserProfile updated);
    Task<(List<UserProfile> Items, int TotalCount)> SearchAsync(int page, int pageSize, string? search, int? rollNo, UserRole? role);
    Task<UserProfile?> GetByMemberNumberAsync(int memberNumber);
    Task<List<UserProfile>> GetByUserIdsAsync(List<Guid> userIds);
    Task<UserProfile?> AdminUpdateNameAsync(Guid userId, string fullName);

}