namespace Form.Interfaces;

public interface IPasswordResetService
{
    Task<(string rawToken, int expiryMinutes)> GenerateAsync(Guid userId);
    Task<bool> ValidateAsync(Guid userId, string code);  

}