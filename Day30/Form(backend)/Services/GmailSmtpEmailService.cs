using System.Net;
using System.Net.Mail;
using Form.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Form.Services;

public class GmailSmtpEmailService : IEmailService
{
    private readonly IConfiguration _config;

    public GmailSmtpEmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendPasswordResetCodeAsync(string toEmail, string code, int expiryMinutes)
    {
        var senderEmail = _config["Email:GmailAddress"]!;
        var appPassword = _config["Email:GmailAppPassword"]!;

        using var client = new SmtpClient("smtp.gmail.com", 587)
        {
            Credentials = new NetworkCredential(senderEmail, appPassword),
            EnableSsl = true,
        };

        var message = new MailMessage
        {
            From = new MailAddress(senderEmail, "Form App"),
            Subject = "Reset your password",
    Body = $"Put the OTP in the password section. This password expires in {expiryMinutes} minutes.\n\n{code}",
            IsBodyHtml = false,
        };
        message.To.Add(toEmail);

        await client.SendMailAsync(message);
    }
}