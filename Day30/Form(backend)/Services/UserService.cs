using Form.DTOs;
using Form.Entities;
using Form.Interfaces;

namespace Form.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    private readonly IUserProfileRepository _profileRepository;

    public UserService(IUserRepository userRepository, IPasswordHasher passwordHasher, IUserProfileRepository profileRepository)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _profileRepository = profileRepository;
    }

    public async Task<List<UserSummaryDtos>> GetStudentsAsync()
    {
        var students = await _userRepository.GetByRoleAsync(UserRole.Student);
        var profiles = await _profileRepository.GetByUserIdsAsync(students.Select(s => s.Id).ToList());
        var nameByUserId = profiles.ToDictionary(p => p.UserId, profiles => profiles.FullName);

        return students.Select(u => new UserSummaryDtos
        {
            Id = u.Id,
            Email = u.Email,
            FullName = nameByUserId.TryGetValue(u.Id, out var name) ? name : u.Email,
        }).ToList();
    }


    public async Task<CreatedUserDto> CreateUserAsync(CreateUserRequest request)
    {
        // Same duplicate-email guard as AuthService.RegisterAsync — one account
        // per email, whether it was self-registered or admin-created.
        var students = (await _userRepository.GetByRoleAsync(UserRole.Student))
    .Where(s => s.IsActive)
    .ToList();
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
        var email = request.Email.Trim();

        var user = new User
        {
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.TemporaryPassword),
        };

        foreach (var role in parsedRoles.Distinct())
        {
            user.RoleAssignments.Add(new UserRoleAssignment { Id = Guid.NewGuid(), Role = role });
        }

        // Created in the SAME save as the user, so the roll number (MemberNumber, an identity
        // column) exists from the first moment. EF fills UserId through the navigation.
        user.Profile = new UserProfile
        {
            FullName = string.IsNullOrWhiteSpace(request.FullName) ? email.Split('@')[0] : request.FullName.Trim(),
            Address = string.Empty,
            PhoneNumbers = new List<string>(),
        };

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