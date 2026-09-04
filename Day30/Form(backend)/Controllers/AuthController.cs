using Form.DTOs;
using Form.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Form.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IConfiguration _config;
    public AuthController(IAuthService authService, IConfiguration config)
    {
        _authService = authService;
        _config = config;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (result is null) return Unauthorized("Invalid email or password.");

        SetAuthCookie(result.Token, result.ExpiresAt);
        SetRefreshCookie(result.RefreshToken, result.RefreshTokenExpiresAt);
        // No longer send the raw token in the body — only non-sensitive info
        return Ok(new { email = result.Email, expiresAt = result.ExpiresAt, roles = result.Roles });
    }

    [HttpPost("register")]

    // logout
    // now genuinely needs the server, since JS can't clear an HttpOnly cookie.
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()   // CHANGE — now async, since we're doing DB work
    {
        if (Request.Cookies.TryGetValue("refreshToken", out var rawRefreshToken)
            && !string.IsNullOrEmpty(rawRefreshToken))
        {
            await _authService.LogoutAsync(rawRefreshToken);   // ADD — actually revoke in the DB
        }

        Response.Cookies.Delete("jwt");
        Response.Cookies.Delete("refreshToken", new CookieOptions { Path = "/api/auth" });
        return Ok();
    }

    // logged in," it asks the server directly. Called once when the app loads.
    [Authorize]
    [HttpGet("me")]
    public ActionResult<object> Me()
    {
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                 ?? User.FindFirst("email")?.Value;
        var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();
        return Ok(new { email ,roles });
    }

    private void SetAuthCookie(string token, DateTime expiresAt)
    {
        Response.Cookies.Append("jwt", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = expiresAt,
        });
    }
    private void SetRefreshCookie(string token, DateTime expiresAt)
    {
        Response.Cookies.Append("refreshToken", token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = expiresAt,
            Path = "/api/auth",  
        });
    }
    [AllowAnonymous]  
    [HttpPost("refresh")]
    public async Task<ActionResult<object>> Refresh()
    {
        if (!Request.Cookies.TryGetValue("refreshToken", out var rawRefreshToken)
            || string.IsNullOrEmpty(rawRefreshToken))
        {
            return Unauthorized();
        }

        var result = await _authService.RefreshAsync(rawRefreshToken);
        if (result is null)
        {
            // Refresh failed (expired, revoked, or reuse-detected). Clear
            // whatever cookies remain — there's no session left to recover.
            Response.Cookies.Delete("jwt");
            Response.Cookies.Delete("refreshToken", new CookieOptions { Path = "/api/auth" });
            return Unauthorized();
        }

        SetAuthCookie(result.Token, result.ExpiresAt);
        SetRefreshCookie(result.RefreshToken, result.RefreshTokenExpiresAt);

        return Ok(new { email = result.Email, expiresAt = result.ExpiresAt , role = result.Roles});
    }

    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        var frontendBaseUrl = _config["Frontend:BaseUrl"]!;
        await _authService.ForgotPasswordAsync(request.Email);

        // ALWAYS the same generic response — see the reasoning above.
        return Ok(new { message = "If an account with that email exists, a reset link has been sent." });
    }
    public class ResetPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        var success = await _authService.ResetPasswordAsync(request.Email, request.Code, request.NewPassword);
        if (!success) return BadRequest("Invalid or expired code.");

        return Ok(new { message = "Password reset successfully." });
    }
}