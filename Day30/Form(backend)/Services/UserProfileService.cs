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

        // FullName defaults to email until the person actually edits it —
        // never leave it blank, an empty name looks broken in the UI.
        var created = await _repository.AddAsync(new UserProfile
        {
            UserId = userId,
            FullName = email,
            Phone = string.Empty,
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

        var updated = await _repository.UpdateAsync(userId, new UserProfile
        {
            FullName = request.FullName,
            Phone = request.Phone,
            AvatarUrl = avatarUrl ?? string.Empty, // empty = "no new avatar", same convention as submissions
        });

        return updated is null ? null : MapToDto(updated);
    }

    public async Task<UserProfileDto?> GetByUserIdAsync(Guid userId)
    {
        var profile = await _repository.GetByUserIdAsync(userId);
        return profile is null ? null : MapToDto(profile);
    }

    public async Task<PagedResult<UserProfileDto>> SearchAsync(int page, int pageSize, string? search)
    {
        var (items, totalCount) = await _repository.SearchAsync(page, pageSize, search);
        return new PagedResult<UserProfileDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    // Separate rules from SubmissionService.ValidateFile on purpose — an avatar
    // has no legitimate reason to be a PDF, and 5MB is oversized for a profile
    // picture. Duplicated here rather than shared for now; if a third file-type
    // shows up later, THAT'S the signal to extract a shared FileValidator class —
    // not before, since two similar-but-not-identical rule sets isn't real duplication yet.
    private static void ValidateAvatar(IFormFile file)
    {
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!allowedExtensions.Contains(extension))
            throw new InvalidOperationException("Avatar must be a JPG, PNG, or WEBP image.");

        const long maxSizeBytes = 2 * 1024 * 1024; // 2 MB — smaller cap than submission attachments
        if (file.Length > maxSizeBytes)
            throw new InvalidOperationException("Avatar too large. Max size is 2MB.");
    }

    private static UserProfileDto MapToDto(UserProfile p) => new()
    {
        UserId = p.UserId,
        Email = p.User.Email,
        FullName = p.FullName,
        Phone = p.Phone,
        AvatarUrl = p.AvatarUrl,
        MemberNumber = p.MemberNumber,
    };
}