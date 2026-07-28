namespace Day_9.Services
{
    public class MockEmailService : IEmailService
    {
        public Task SendEmailAsync(string to ,string subject , string body)
        {
            Console.WriteLine($"Email will be sent to:{to}");
            return Task.CompletedTask;
        }
    }
}
