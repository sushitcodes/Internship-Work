using System.Text;
using System.Text.Json;
using BookApi.Models;

namespace BookApi.Consumer
{
    public class BookApiClient
    {
        private readonly HttpClient _httpClient;

        // JSON options
        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public BookApiClient(string baseUrl = "https://localhost:7085")
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(baseUrl);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        // ==========================
        // GET ALL BOOKS
        // ==========================
        public async Task<List<Book>> GetAllBooksAsync()
        {
            var response = await _httpClient.GetAsync("/api/books");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            Console.WriteLine("\n========== RAW JSON ==========");
            Console.WriteLine(json);
            Console.WriteLine("==============================\n");

            return JsonSerializer.Deserialize<List<Book>>(json, _jsonOptions)!;
        }

        // ==========================
        // GET BOOK BY ID
        // ==========================
        public async Task<Book?> GetBookByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"/api/books/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return null;

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            Console.WriteLine("\n========== RAW JSON ==========");
            Console.WriteLine(json);
            Console.WriteLine("==============================\n");

            return JsonSerializer.Deserialize<Book>(json, _jsonOptions);
        }

        // ==========================
        // CREATE BOOK
        // ==========================
        public async Task<Book?> CreateBookAsync(Book book)
        {
            var json = JsonSerializer.Serialize(book);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("/api/books", content);

            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync();

            Console.WriteLine("\n========== RAW JSON ==========");
            Console.WriteLine(responseJson);
            Console.WriteLine("==============================\n");

            return JsonSerializer.Deserialize<Book>(responseJson, _jsonOptions);
        }

        // ==========================
        // UPDATE BOOK
        // ==========================
        public async Task<bool> UpdateBookAsync(Book book)
        {
            var json = JsonSerializer.Serialize(book);

            var content = new StringContent(
                json,
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PutAsync($"/api/books/{book.Id}", content);

            return response.IsSuccessStatusCode;
        }

        // ==========================
        // DELETE BOOK
        // ==========================
        public async Task<bool> DeleteBookAsync(int id)
        {
            var response = await _httpClient.DeleteAsync($"/api/books/{id}");

            return response.IsSuccessStatusCode;
        }
    }
}