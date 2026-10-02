using Form.DTOs;
using Form.Interfaces;

namespace Form.Services;

public class AuthService(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    ITokenService tokenService,
    IRefreshTokenService refreshTokenService,
    IPasswordResetService passwordResetService,
    EmailQueue emailQueue) : IAuthService
{
    // A real BCrypt hash of a throwaway string. Verifying against it when the email is unknown
    // makes "unknown email" cost the same time as "wrong password".
    private static readonly string DummyHash =
        BCrypt.Net.BCrypt.HashPassword("timing-equaliser-not-a-real-password");

    public async Task ForgotPasswordAsync(string email)
    {
        var user = await userRepository.GetByEmailAsync(email.Trim());
        if (user is null) return;

        var (code, expiryMinutes) = await passwordResetService.GenerateAsync(user.Id);

        // Queued, not sent here: SMTP is slow and only happens for real accounts,
        // which would reveal them through response time.
        emailQueue.Enqueue(new EmailJob(user.Email, code, expiryMinutes));
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginRequest request)
    {
        var user = await userRepository.GetByEmailAsync(request.Email.Trim());

        // ALWAYS run one BCrypt verify, even when the user does not exist.
        var passwordOk = passwordHasher.Verify(request.Password, user?.PasswordHash ?? DummyHash);
        if (user is null || !passwordOk)
            return null;   // deliberately vague

        if (!user.IsActive)
            throw new InvalidOperationException("This account has been deactivated. Contact an administrator.");

        var (token, expiresAt) = tokenService.CreateToken(user);
        var (refreshToken, refreshExpiresAt) = await refreshTokenService.GenerateAsync(user.Id);

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

    public async Task<AuthResponseDto?> RefreshAsync(string rawRefreshToken)
    {
        var result = await refreshTokenService.ValidateAndRotateAsync(rawRefreshToken);
        if (!result.Success || result.User is null)
            return null;

        var (accessToken, accessExpiresAt) = tokenService.CreateToken(result.User);

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

    public async Task LogoutAsync(string rawRefreshToken) =>
        await refreshTokenService.RevokeAsync(rawRefreshToken);

    public async Task<bool> ResetPasswordAsync(string email, string code, string newPassword)
    {
        var user = await userRepository.GetByEmailAsync(email.Trim());
        if (user is null) return false;

        var isValid = await passwordResetService.ValidateAsync(user.Id, code);
        if (!isValid) return false;

        user.PasswordHash = passwordHasher.Hash(newPassword);
        await userRepository.UpdateAsync(user);

        // A password change ends every existing session.
        await refreshTokenService.RevokeAllForUserAsync(user.Id);
        return true;
    }
}