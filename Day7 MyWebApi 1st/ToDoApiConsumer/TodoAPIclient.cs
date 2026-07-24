using System.Text;
using System.Text.Json;

public class TodoApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public TodoApiClient(string baseUrl = "http://192.168.1.69:5284")
        {
            _baseUrl = baseUrl;
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(_baseUrl);
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/json");
        }

        // ========== GET ALL TODOS ==========
        public async Task<List<Todo>> GetAllTodosAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("/api/todos");
                response.EnsureSuccessStatusCode(); // Throws if not 2xx

                var json = await response.Content.ReadAsStringAsync();
            //Console.WriteLine(json);
            return JsonSerializer.Deserialize<List<Todo>>(json,options);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"❌ Error getting todos: {ex.Message}");
                return new List<Todo>();
            }
        }


        // ========== GET TODO BY ID ==========
        public async Task<Todo> GetTodoByIdAsync(int id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/api/todos/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        Console.WriteLine($"❌ Todo with ID {id} not found");
                        return null;
                    }
                    return null;
                }

                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Todo>(json,options);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"❌ Error getting todo {id}: {ex.Message}");
                return null;
            }
        }

        // ========== CREATE NEW TODO ==========
        public async Task<Todo> CreateTodoAsync(Todo todo)
        {
            try
            {
                var json = JsonSerializer.Serialize(todo);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/api/todos", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<Todo>(responseJson);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"❌ Error creating todo: {ex.Message}");
                return null;
            }
        }

        // ========== UPDATE TODO ==========
        public async Task<bool> UpdateTodoAsync(Todo todo)
        {
            try
            {
                var json = JsonSerializer.Serialize(todo);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync($"/api/todos/{todo.Id}", content);
                response.EnsureSuccessStatusCode();

                return true;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"❌ Error updating todo {todo.Id}: {ex.Message}");
                return false;
            }
        }

        // ========== DELETE TODO ==========
        public async Task<bool> DeleteTodoAsync(int id)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"/api/todos/{id}");
                response.EnsureSuccessStatusCode();

                return true;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"❌ Error deleting todo {id}: {ex.Message}");
                return false;
            }
        }

        // ========== MARK TODO AS COMPLETE ==========
        public async Task<bool> CompleteTodoAsync(int id)
        {
            try
            {
                var response = await _httpClient.PatchAsync($"/api/todos/{id}/complete", null);
                response.EnsureSuccessStatusCode();

                return true;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"❌ Error completing todo {id}: {ex.Message}");
                return false;
            }
        }
    private static readonly JsonSerializerOptions options = new()
    {
        PropertyNameCaseInsensitive = true
    };
}