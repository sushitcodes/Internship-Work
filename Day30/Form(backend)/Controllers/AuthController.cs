using Form.DTOs;
using Form.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;

namespace Form.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService _authService, IConfiguration _config) : ControllerBase
{
    // One place decides cookie attributes. Set and Delete MUST use identical options,
    // otherwise the browser treats them as different cookies and logout can silently fail.
    private CookieOptions BuildCookieOptions(DateTimeOffset? expires, string path = "/")
    {
        var sameSite = Enum.TryParse<SameSiteMode>(_config["Cookies:SameSite"], true, out var parsed)
            ? parsed
            : SameSiteMode.Lax;

        return new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = sameSite,
            Expires = expires,
            Path = path,
        };
    }

    private void SetAuthCookie(string token, DateTime expiresAt) =>
        Response.Cookies.Append("jwt", token, BuildCookieOptions(expiresAt));

    private void SetRefreshCookie(string token, DateTime expiresAt) =>
        Response.Cookies.Append("refreshToken", token, BuildCookieOptions(expiresAt, "/api/auth"));

    private void ClearAuthCookies()
    {
        Response.Cookies.Delete("jwt", BuildCookieOptions(null));
        Response.Cookies.Delete("refreshToken", BuildCookieOptions(null, "/api/auth"));
    }

    [HttpPost("login")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginRequest request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            if (result is null) return Unauthorized("Invalid email or password.");

            SetAuthCookie(result.Token, result.ExpiresAt);
            SetRefreshCookie(result.RefreshToken, result.RefreshTokenExpiresAt);
            return Ok(new { email = result.Email, expiresAt = result.ExpiresAt, roles = result.Roles });
        }
        catch (InvalidOperationException ex)
        {
            return Unauthorized(ex.Message); // deactivated account: specific readable message
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        if (Request.Cookies.TryGetValue("refreshToken", out var rawRefreshToken)
            && !string.IsNullOrEmpty(rawRefreshToken))
        {
            await _authService.LogoutAsync(rawRefreshToken);
        }

        ClearAuthCookies();
        return Ok();
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<object> Me()
    {
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                 ?? User.FindFirst("email")?.Value;
        var roles = User.FindAll(System.Security.Claims.ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

        // Read the token expiry from the JWT claim so the frontend can schedule
        // a proactive refresh without waiting for a 401.
        DateTime? expiresAt = null;
        var expClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Expiration)?.Value
                    ?? User.FindFirst("exp")?.Value;
        if (long.TryParse(expClaim, out var expUnix))
            expiresAt = DateTimeOffset.FromUnixTimeSeconds(expUnix).UtcDateTime;

        return Ok(new { email, roles, expiresAt });
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
            ClearAuthCookies();
            return Unauthorized();
        }

        SetAuthCookie(result.Token, result.ExpiresAt);
        SetRefreshCookie(result.RefreshToken, result.RefreshTokenExpiresAt);

        // "roles" (plural) to match /login and /me. Was "role" before.
        return Ok(new { email = result.Email, expiresAt = result.ExpiresAt, roles = result.Roles });
    }

    public class ForgotPasswordRequest
    {
        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordRequest request)
    {
        await _authService.ForgotPasswordAsync(request.Email);

        // ALWAYS the same generic response so this endpoint cannot be used to discover accounts.
        return Ok(new { message = "If an account with that email exists, a reset link has been sent." });
    }

    public class ResetPasswordRequest
    {
        [Required, EmailAddress, StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(6, MinimumLength = 6)]
        public string Code { get; set; } = string.Empty;

        // 72 because BCrypt ignores everything after 72 bytes.
        [Required, StringLength(72, MinimumLength = 8)]
        public string NewPassword { get; set; } = string.Empty;
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    [EnableRateLimiting("AuthPolicy")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        var success = await _authService.ResetPasswordAsync(request.Email, request.Code, request.NewPassword);
        if (!success) return BadRequest("Invalid or expired code.");

        return Ok(new { message = "Password reset successfully." });
    }
}