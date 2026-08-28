using Form.DTOs;

namespace Form.Interface
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequest request);
        Task<AuthResponseDto?> LoginAsync(LoginRequest request);
        Task<AuthResponseDto?> RefreshAsync(string rawRefreshToken);
        Task LogoutAsync(string rawRefreshToken);
        Task ForgotPasswordAsync(string email, string frontendBaseUrl);


    }
}
