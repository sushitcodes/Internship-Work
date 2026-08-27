using Form.DTOs;
using Form.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.InteropServices;

namespace Form.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        if (result is null) return Unauthorized("Invalid email or password.");

        SetAuthCookie(result.Token, result.ExpiresAt);
        SetRefreshCookie(result.RefreshToken, result.RefreshTokenExpiresAt);
        // No longer send the raw token in the body — only non-sensitive info
        return Ok(new { email = result.Email, expiresAt = result.ExpiresAt });
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterRequest request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);
            SetAuthCookie(result.Token, result.ExpiresAt);
            SetRefreshCookie(result.RefreshToken, result.RefreshTokenExpiresAt);

            return Ok(new { email = result.Email, expiresAt = result.ExpiresAt });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // logout
    // now genuinely needs the server, since JS can't clear an HttpOnly cookie.
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("jwt");
        return Ok();
    }

    // logged in," it asks the server directly. Called once when the app loads.
    [Authorize]
    [HttpGet("me")]
    public ActionResult<object> Me()
    {
        var email = User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value
                 ?? User.FindFirst("email")?.Value;
        return Ok(new { email });
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

        return Ok(new { email = result.Email, expiresAt = result.ExpiresAt });
    }
}