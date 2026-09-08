using Form.DTOs;
using Form.Interfaces;
using Form.Entities;


namespace Form.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly IPasswordResetService _passwordResetService;
    private readonly IEmailService _emailService;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IRefreshTokenService refreshTokenService,
         IPasswordResetService passwordResetService,   
    IEmailService emailService)

    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _refreshTokenService = refreshTokenService;
        _passwordResetService = passwordResetService; 
        _emailService = emailService;
    }
    public async Task ForgotPasswordAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user is null) return;

        var (code, expiryMinutes) = await _passwordResetService.GenerateAsync(user.Id);
        await _emailService.SendPasswordResetCodeAsync(user.Email, code, expiryMinutes);
    }

    //public async Task ForgotPasswordAsync(string email, string frontendBaseUrl)
    //{
    //    var user = await _userRepository.GetByEmailAsync(email);

    //    // Deliberately do nothing detectable if the user doesn't exist —
    //    // same "don't leak which emails are registered" principle as Login.
    //    if (user is null) return;

    //    var (rawToken, expiryMinutes) = await _passwordResetService.GenerateAsync(user.Id);
    //    var resetLink = $"{frontendBaseUrl}/reset-password?token={rawToken}";

    //    await _emailService.SendPasswordResetEmailAsync(user.Email, resetLink, expiryMinutes);
    //}
    public async Task<AuthResponseDto> RegisterAsync(RegisterRequest request)
    {
        var existing = await _userRepository.GetByEmailAsync(request.Email);
        if (existing is not null)
            throw new InvalidOperationException("An account with this email already exists.");

        var user = new User
        {
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
        };
        user.RoleAssignments.Add(new UserRoleAssignment { Id = Guid.NewGuid(), Role = UserRole.Student });

        var saved = await _userRepository.AddAsync(user);

        var (token, expiresAt) = _tokenService.CreateToken(saved);
        var (refreshToken, refreshExpiresAt) = await _refreshTokenService.GenerateAsync(saved.Id);
        return new AuthResponseDto 
        { Token = token,
            Email = saved.Email, 
            ExpiresAt = expiresAt,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = refreshExpiresAt,
            Roles = saved.RoleAssignments.Select(ra => ra.Role.ToString()).ToList(),

        };

      

    }

    public async Task<AuthResponseDto?> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            return null; // deliberately vague — don't reveal which part was wrong
                         // Was missing entirely — this is the actual check that never existed.
        if (!user.IsActive)
            throw new InvalidOperationException("This account has been deactivated. Contact an administrator.");

        var (token, expiresAt) = _tokenService.CreateToken(user);
        var (refreshToken, refreshExpiresAt) = await _refreshTokenService.GenerateAsync(user.Id);

        return new AuthResponseDto
        {
            Token = token,
            Email = user.Email,
            ExpiresAt = expiresAt,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = refreshExpiresAt,
            Roles = user.RoleAssignments.Select(ra => ra.Role.ToString()).ToList(),
        };
    }
    // Form.Services/AuthService.cs
    public async Task<AuthResponseDto?> RefreshAsync(string rawRefreshToken)
    {
        var result = await _refreshTokenService.ValidateAndRotateAsync(rawRefreshToken);
        if (!result.Success || result.User is null)
            return null;

        var (accessToken, accessExpiresAt) = _tokenService.CreateToken(result.User);

        return new AuthResponseDto
        {
            Token = accessToken,
            Email = result.User.Email,
            ExpiresAt = accessExpiresAt,
            RefreshToken = result.NewRawToken!,
            RefreshTokenExpiresAt = result.NewExpiresAt!.Value,
            Roles = result.User.RoleAssignments.Select(ra => ra.Role.ToString()).ToList(),
        };
    }
    public async Task LogoutAsync(string rawRefreshToken)
    {
        await _refreshTokenService.RevokeAsync(rawRefreshToken);
    }
    // AuthService.cs
    public async Task<bool> ResetPasswordAsync(string email, string code, string newPassword)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        if (user is null) return false;

        var isValid = await _passwordResetService.ValidateAsync(user.Id, code);
        if (!isValid) return false;

        user.PasswordHash = _passwordHasher.Hash(newPassword);
        await _userRepository.UpdateAsync(user);   // see note below

        // A password change should invalidate every existing session —
        // if someone's account was compromised, this locks the attacker
        // out of any device they were already logged into.
        await _refreshTokenService.RevokeAllForUserAsync(user.Id);

        return true;
    }
}