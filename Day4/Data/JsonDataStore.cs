using System.Text.Json;

namespace LibraryManagementSystem.Data
{
    // A GENERIC, reusable JSON store. Works for Book, Member, or BorrowRecord —
    // or any future model — without writing separate load/save code for each.
    // This is the file-handling equivalent of what DatabaseHelper was for ADO.NET.
    public class JsonDataStore<T>
    {
        private readonly string _filePath;
        private static readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true // pretty-print so the .json files are readable if you open them
        };

        public JsonDataStore(string fileName)
        {
            string dataFolder = @"G:\basic C#\Day4\data";
            Directory.CreateDirectory(dataFolder); // creates it if it doesn't exist yet

            _filePath = Path.Combine(dataFolder, fileName);
        }

        // Reads the whole file into a List<T>. Returns an empty list if the
        // file doesn't exist yet (first run) rather than throwing.
        public List<T> Load()
        {
            if (!File.Exists(_filePath))
            {
                return new List<T>();
            }

            string json = File.ReadAllText(_filePath);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<T>();
            }

            return JsonSerializer.Deserialize<List<T>>(json, _options) ?? new List<T>();
        }

        // Overwrites the file with the full current list.
        // Simple strategy: rewrite everything on every save. Fine for this
        // scale of app; a real system would use a proper database instead.
        public void Save(List<T> items)
        {
            string json = JsonSerializer.Serialize(items, _options);
            File.WriteAllText(_filePath, json);
        }
    }
}
