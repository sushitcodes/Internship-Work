namespace Form.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetCodeAsync(string toEmail, string code, int expiryMinutes);
}