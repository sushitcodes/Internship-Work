using BookApi.Models;
namespace BookApi.Consumer
{
    class Program
    {
         static async Task Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            Console.WriteLine("═══════════════════════════════════════");
            Console.WriteLine("   📚 BOOK API CONSUMER");
            Console.WriteLine("═══════════════════════════════════════\n");

            var client = new BookApiClient("http://localhost:5017");

            try
            {
                // 1. GET all books
                Console.WriteLine("📋 1. GETTING ALL BOOKS:");
                var books = await client.GetAllBooksAsync();
                DisplayBooks(books);
                Console.WriteLine();

                // 2. GET book by ID
                Console.WriteLine("🔍 2. GETTING BOOK BY ID (ID=1):");
                var book = await client.GetBookByIdAsync(1);
                if (book != null)
                    DisplayBook(book);
                Console.WriteLine();

                // 3. CREATE new book
                Console.WriteLine("➕ 3. CREATING NEW BOOK:");
                var newBook = new Book
                {
                    Title = "The Alchemist",
                    Author = "Paulo Coelho",
                    Genre = "Fiction",
                    IsRead = false,
                    Rating = 0
                };
                var created = await client.CreateBookAsync(newBook);
                Console.WriteLine($"✅ Created: {created.Title} (ID: {created.Id})");
                Console.WriteLine();

                // 4. UPDATE book
                Console.WriteLine("✏️ 4. UPDATING BOOK (ID=1):");
                var updateBook = await client.GetBookByIdAsync(1);
                if (updateBook != null)
                {
                    updateBook.Title = "Clean Code (Updated)";
                    updateBook.IsRead = true;
                    updateBook.Rating = 5;
                    var updated = await client.UpdateBookAsync(updateBook);
                    Console.WriteLine(updated ? "✅ Book updated!" : "❌ Update failed");
                }
                Console.WriteLine();

                // 5. DELETE book
                Console.WriteLine("🗑️ 5. DELETING BOOK (ID=3):");
                var deleted = await client.DeleteBookAsync(3);
                Console.WriteLine(deleted ? "✅ Book deleted!" : "❌ Delete failed");
                Console.WriteLine();

                // 6. Final list
                Console.WriteLine("📋 6. FINAL BOOK LIST:");
                var finalBooks = await client.GetAllBooksAsync();
                DisplayBooks(finalBooks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                Console.WriteLine("Make sure the API is running at https://localhost:7269");
            }

            Console.WriteLine("\n═══════════════════════════════════════");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static void DisplayBooks(List<Book> books)
        {
            if (books == null || books.Count == 0)
            {
                Console.WriteLine("  No books found.");
                return;
            }

            foreach (var book in books)
            {
                var status = book.IsRead ? "✅ Read" : "📖 Unread";
                Console.WriteLine($"  [{book.Id}] {book.Title} - {status} ⭐ {book.Rating}/5");
            }
            Console.WriteLine($"  Total: {books.Count} books");
        }

        static void DisplayBook(Book book)
        {
            var status = book.IsRead ? "✅ Read" : "📖 Unread";
            Console.WriteLine($"  ID: {book.Id}");
            Console.WriteLine($"  Title: {book.Title}");
            Console.WriteLine($"  Author: {book.Author}");
            Console.WriteLine($"  Genre: {book.Genre}");
            Console.WriteLine($"  Status: {status}");
            Console.WriteLine($"  Rating: {book.Rating}/5");
        }
    }
}