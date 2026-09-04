using Form.DTOs;
using Form.Entities;
using Form.Interfaces;

namespace Form.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<List<UserSummaryDto>> GetStudentsAsync() =>
        (await _userRepository.GetByRoleAsync(UserRole.Student))
            .Select(u => new UserSummaryDto { Id = u.Id, Email = u.Email })
            .ToList();

    public async Task<CreatedUserDto> CreateUserAsync(CreateUserRequest request)
    {
        // Same duplicate-email guard as AuthService.RegisterAsync — one account
        // per email, whether it was self-registered or admin-created.
        var existing = await _userRepository.GetByEmailAsync(request.Email);
        if (existing is not null)
            throw new InvalidOperationException("An account with this email already exists.");

        if (request.Roles.Count == 0)
            throw new InvalidOperationException("At least one role must be selected.");

        // Parse each role NAME into the real enum, failing loudly on anything
        // unrecognized rather than silently skipping a typo'd role.
        var parsedRoles = new List<UserRole>();
        foreach (var roleName in request.Roles)
        {
            if (!Enum.TryParse<UserRole>(roleName, ignoreCase: true, out var parsed))
                throw new InvalidOperationException($"Unknown role: {roleName}");
            parsedRoles.Add(parsed);
        }

        var user = new User
        {
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.TemporaryPassword),
        };

        foreach (var role in parsedRoles.Distinct())
        {
            user.RoleAssignments.Add(new UserRoleAssignment { Id = Guid.NewGuid(), Role = role });
        }

        var saved = await _userRepository.AddAsync(user);

        // Deliberately NOT creating a UserProfile row here. UserProfileService
        // already handles this lazily via GetOrCreateOwnProfileAsync — the
        // first time this new user (or an Admin browsing their profile) hits
        // GET /users/me/profile or /{id}/profile, the profile gets created
        // then. One creation path instead of two, avoiding drift between them.

        return new CreatedUserDto
        {
            Id = saved.Id,
            Email = saved.Email,
            Roles = saved.RoleAssignments.Select(ra => ra.Role.ToString()).ToList(),
        };
    }
}