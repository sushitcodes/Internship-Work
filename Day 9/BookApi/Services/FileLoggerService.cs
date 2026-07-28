namespace Day_9.Services
{
    public class FileLoggerService : ILogginService
    {

        private readonly string _filePath = "log.txt";

        public void LogInfo(string message)
        {
            File.AppendAllText(_filePath, $"Info:{message}\r\n");
        }
        public void LogError(string message)
        {
            File.AppendAllText(_filePath, $"Error:{message}\r\n");
        }

       
    }
}
