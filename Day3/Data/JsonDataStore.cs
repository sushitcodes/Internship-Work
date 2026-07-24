using System.Text.Json;

namespace SchoolManagementSystem.Data
{
    // GENERICS + ASYNC/AWAIT — a generic store, but this time using the
    // async file APIs so loading/saving doesn't block the calling thread.
    public class JsonDataStore<T>
    {
        private readonly string _filePath;
        private static readonly JsonSerializerOptions _options = new() { WriteIndented = true };

        public JsonDataStore(string fileName)
        {
            string dataFolder = @"G:\basic C#\Day3\Data";
            Directory.CreateDirectory(dataFolder); // creates it if it doesn't exist yet

            _filePath = Path.Combine(dataFolder, fileName);
        }

        // ASYNC/AWAIT — File I/O is a classic case for async: while the OS
        // reads the disk, the thread is freed up instead of sitting blocked.
        public async Task<List<T>> LoadAsync()
        {
            if (!File.Exists(_filePath)) return new List<T>();

            string json = await File.ReadAllTextAsync(_filePath);
            if (string.IsNullOrWhiteSpace(json)) return new List<T>();

            return JsonSerializer.Deserialize<List<T>>(json, _options) ?? new List<T>();
        }

        public async Task SaveAsync(List<T> items)
        {
            string json = JsonSerializer.Serialize(items, _options);
            await File.WriteAllTextAsync(_filePath, json);
        }
    }
}
