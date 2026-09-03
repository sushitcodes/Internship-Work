using Form.DTOs;

namespace Form.Interfaces;

public interface IUserProfileService
{
    Task<UserProfileDto> GetOrCreateOwnProfileAsync(Guid userId, string email);
    Task<UserProfileDto?> UpdateOwnProfileAsync(Guid userId, UpdateOwnProfileRequest request);
    Task<UserProfileDto?> GetByUserIdAsync(Guid userId);
    Task<PagedResult<UserProfileDto>> SearchAsync(int page, int pageSize, string? search);
}