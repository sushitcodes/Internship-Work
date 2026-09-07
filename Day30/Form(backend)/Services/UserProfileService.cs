using Form.DTOs;
using Form.Interfaces;
using Form.Entities;

namespace Form.Services;

public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _repository;
    private readonly IFileStorageService _fileStorage;

    public UserProfileService(IUserProfileRepository repository, IFileStorageService fileStorage)
    {
        _repository = repository;
        _fileStorage = fileStorage;
    }

    public async Task<UserProfileDto> GetOrCreateOwnProfileAsync(Guid userId, string email)
    {
        var existing = await _repository.GetByUserIdAsync(userId);
        if (existing is not null) return MapToDto(existing);

        // Derived name (before '@') — only for brand-new profiles, per
        // your earlier decision. Existing profiles never hit this branch.
        var derivedName = email.Contains('@') ? email[..email.IndexOf('@')] : email;

        var created = await _repository.AddAsync(new UserProfile
        {
            UserId = userId,
            FullName = derivedName,
            Address = string.Empty,
            PhoneNumbers = new List<string>(),
        });

        return MapToDto(created);
    }

    public async Task<UserProfileDto?> UpdateOwnProfileAsync(Guid userId, UpdateOwnProfileRequest request)
    {
        string? avatarUrl = null;
        if (request.Avatar is not null)
        {
            ValidateAvatar(request.Avatar);
            avatarUrl = await _fileStorage.SaveFileAsync(request.Avatar);
        }

        Gender? parsedGender = null;
        if (!string.IsNullOrWhiteSpace(request.Gender))
        {
            if (!Enum.TryParse<Gender>(request.Gender, ignoreCase: true, out var g))
                throw new InvalidOperationException($"Unknown gender: {request.Gender}");
            parsedGender = g;
        }

        var updated = await _repository.UpdateAsync(userId, new UserProfile
        {
            Address = request.Address,
            Gender = parsedGender,
            PhoneNumbers = request.PhoneNumbers,
            AvatarUrl = avatarUrl ?? string.Empty,
            // FullName intentionally absent — self-edit can never touch it.
        });

        return updated is null ? null : MapToDto(updated);
    }

    public async Task<UserProfileDto?> GetByUserIdAsync(Guid userId)
    {
        var profile = await _repository.GetByUserIdAsync(userId);
        return profile is null ? null : MapToDto(profile);
    }

    public async Task<PagedResult<UserProfileDto>> SearchAsync(int page, int pageSize, string? search, int? rollNo, string? role)
    {
        UserRole? parsedRole = null;
        if (!string.IsNullOrWhiteSpace(role))
        {
            if (!Enum.TryParse<UserRole>(role, ignoreCase: true, out var r))
                throw new InvalidOperationException($"Unknown role: {role}");
            parsedRole = r;
        }

        var (items, totalCount) = await _repository.SearchAsync(page, pageSize, search, rollNo, parsedRole);
        return new PagedResult<UserProfileDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    private static void ValidateAvatar(IFormFile file)
    {
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            throw new InvalidOperationException("Avatar must be a JPG, PNG, or WEBP image.");

        const long maxSizeBytes = 2 * 1024 * 1024;
        if (file.Length > maxSizeBytes)
            throw new InvalidOperationException("Avatar too large. Max size is 2MB.");
    }

    private static UserProfileDto MapToDto(UserProfile p) => new()
    {
        UserId = p.UserId,
        Email = p.User.Email,
        FullName = p.FullName,
        Address = p.Address,
        Gender = p.Gender?.ToString(),
        PhoneNumbers = p.PhoneNumbers,
        AvatarUrl = p.AvatarUrl,
        MemberNumber = p.MemberNumber,
    };
}