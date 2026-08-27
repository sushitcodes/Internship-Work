using Form.DTOs;
using Form.Entities;
using Form.Interface;

namespace Form.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenService _refreshTokenService;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IRefreshTokenService refreshTokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _refreshTokenService = refreshTokenService;
    }

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
        var saved = await _userRepository.AddAsync(user);

        var (token, expiresAt) = _tokenService.CreateToken(saved);
        var (refreshToken, refreshExpiresAt) = await _refreshTokenService.GenerateAsync(saved.Id);
        return new AuthResponseDto 
        { Token = token,
            Email = saved.Email, 
            ExpiresAt = expiresAt,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = refreshExpiresAt,   

        };
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
            return null; // deliberately vague — don't reveal which part was wrong

        var (token, expiresAt) = _tokenService.CreateToken(user);
        var (refreshToken, refreshExpiresAt) = await _refreshTokenService.GenerateAsync(user.Id);

        return new AuthResponseDto { Token = token,
            Email = user.Email,
            ExpiresAt = expiresAt ,
            RefreshToken = refreshToken,             
            RefreshTokenExpiresAt = refreshExpiresAt,
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
        };
    }
}