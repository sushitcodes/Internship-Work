namespace Form.DTOs
{
   public class LoginRequest
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string Email { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Required]
        public string Password { get; set; } = string.Empty;
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string RefreshToken { get; set; } = string.Empty;  
        public DateTime RefreshTokenExpiresAt { get; set; }
public List<string> Roles { get; set; } = new();    
    }
}
