using System.Net;
using System.Net.Mail;
using Form.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Form.Services;

public class GmailSmtpEmailService(IConfiguration config) : IEmailService
{

    public async Task SendPasswordResetCodeAsync(string toEmail, string code, int expiryMinutes)
    {
        var senderEmail = config["Email:GmailAddress"]!;
        var appPassword = config["Email:GmailAppPassword"]!;

        using var client = new SmtpClient("smtp.gmail.com", 587)
        {
            Credentials = new NetworkCredential(senderEmail, appPassword),
            EnableSsl = true,
        };
        using var message = new MailMessage
        {
            From = new MailAddress(senderEmail, "Form App"),
            Subject = "Your password reset code",
            Body = $"Your password reset code is: {code}\n\n" +
                   $"It expires in {expiryMinutes} minutes. If you did not request this, ignore this email.",
            IsBodyHtml = false,
        };
        message.To.Add(toEmail);

        await client.SendMailAsync(message);
    }
}