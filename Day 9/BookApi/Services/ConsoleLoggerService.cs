namespace Day_9.Services
{
    public class ConsoleLoggerService : ILogginService
    {
        public void LogInfo(string message) 
        {
            Console.WriteLine($"Info:{message}");
        }

        public void LogError(string message)
        {
            Console.WriteLine($"Error: {message}");
        }

    }
}
