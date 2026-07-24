class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("   TODO API CONSUMER");
            Console.WriteLine("═══════════════════════════════════════\n");

        // 1. Create API client
        // Use your API URL - if running locally, use localhost
        string apiUrl = "http://192.168.1.69:5284";
        var client = new TodoApiClient(apiUrl);

            // 2. Test all operations
            await TestApiOperations(client);

            Console.WriteLine("\n═══════════════════════════════════════");
            Console.WriteLine("   PRESS ANY KEY TO EXIT");
            Console.ReadKey();
        }

        static async Task TestApiOperations(TodoApiClient client)
        {
            // ========== 1. GET ALL TODOS ==========
            Console.WriteLine("📋 1. GETTING ALL TODOS:");
            var todos = await client.GetAllTodosAsync();
            DisplayTodos(todos);
            Console.WriteLine("\n" + new string('-', 50));

            // ========== 2. GET BY ID ==========
            Console.WriteLine("🔍 2. GETTING TODO BY ID (ID=1):");
            var todo = await client.GetTodoByIdAsync(1);
            if (todo != null)
                DisplayTodo(todo);
            else
                Console.WriteLine("Todo not found");
            Console.WriteLine("\n" + new string('-', 50));

            // ========== 3. CREATE NEW TODO ==========
            Console.WriteLine("➕ 3. CREATING NEW TODO:");
            var newTodo = new Todo
            {
                Title = "Learn API Consumption",
                Description = "Build a console app to consume the Todo API",
                IsCompleted = false
            };

            var createdTodo = await client.CreateTodoAsync(newTodo);
            if (createdTodo != null)
            {
                Console.WriteLine($"✅ Created: {createdTodo.Title} (ID: {createdTodo.Id})");

                // Show all todos again to verify
                Console.WriteLine("\n📋 ALL TODOS (after creation):");
                var updatedTodos = await client.GetAllTodosAsync();
                DisplayTodos(updatedTodos);
            }
            Console.WriteLine("\n" + new string('-', 50));

            // ========== 4. UPDATE TODO ==========
            Console.WriteLine("✏️ 4. UPDATING TODO (ID=1):");
            var todoToUpdate = await client.GetTodoByIdAsync(1);
            if (todoToUpdate != null)
            {
                todoToUpdate.Title = "Learn C# (UPDATED)";
                todoToUpdate.IsCompleted = true;

                var success = await client.UpdateTodoAsync(todoToUpdate);
                if (success)
                {
                    Console.WriteLine($"✅ Updated: {todoToUpdate.Title}");

                    // Verify update
                    var updated = await client.GetTodoByIdAsync(1);
                    Console.WriteLine($"📋 After update: Title = {updated.Title}, Completed = {updated.IsCompleted}");
                }
            }
            Console.WriteLine("\n" + new string('-', 50));

            // ========== 5. MARK TODO AS COMPLETE ==========
            Console.WriteLine("✅ 5. MARKING TODO AS COMPLETE (ID=3):");
            var completeSuccess = await client.CompleteTodoAsync(3);
            if (completeSuccess)
            {
                Console.WriteLine($"✅ Todo 3 marked as complete!");

                // Verify
                var completed = await client.GetTodoByIdAsync(3);
                if (completed != null)
                    Console.WriteLine($"📋 Todo 3: Completed = {completed.IsCompleted}");
            }
            Console.WriteLine("\n" + new string('-', 50));

            // ========== 6. DELETE TODO ==========
            Console.WriteLine("🗑️ 6. DELETING TODO (ID=2):");
            var deleteSuccess = await client.DeleteTodoAsync(2);
            if (deleteSuccess)
            {
                Console.WriteLine($"✅ Todo 2 deleted!");

                // Show final list
                Console.WriteLine("\n📋 FINAL TODOS (after deletion):");
                var finalTodos = await client.GetAllTodosAsync();
                DisplayTodos(finalTodos);
            }
        }

        static void DisplayTodos(List<Todo> todos)
        {
            if (todos == null || todos.Count == 0)
            {
                Console.WriteLine("No todos found.");
                return;
            }

            foreach (var todo in todos)
            {
                var status = todo.IsCompleted ? "✅ Done" : "⏳ Pending";
                Console.WriteLine($"  [{todo.Id}] {todo.Title} - {status}");
            }
            Console.WriteLine($"  Total: {todos.Count} todos");
        }

        static void DisplayTodo(Todo todo)
        {
            var status = todo.IsCompleted ? "✅ Done" : "⏳ Pending";
            Console.WriteLine($"  ID: {todo.Id}");
            Console.WriteLine($"  Title: {todo.Title}");
            Console.WriteLine($"  Description: {todo.Description}");
            Console.WriteLine($"  Status: {status}");
            Console.WriteLine($"  Created: {todo.CreatedAt}");
        }
    }