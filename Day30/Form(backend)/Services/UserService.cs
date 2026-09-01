using Form.DTOs;
using Form.Entities;
using Form.Interfaces;
namespace Form.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    public UserService(IUserRepository userRepository) => _userRepository = userRepository;

    public async Task<List<UserSummaryDto>> GetStudentsAsync() =>
        (await _userRepository.GetByRoleAsync(UserRole.Student))
            .Select(u => new UserSummaryDto { Id = u.Id, Email = u.Email })
            .ToList();
}