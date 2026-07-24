using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;

public class Program
{
    static void Main(string[] args)
    {
        var library = new LibraryService();
        bool running = true;

        while (running)
        {
            PrintMenu();
            string? choice = Console.ReadLine();

            try
            {
                switch (choice)
                {
                    case "1": AddBook(); break;
                    case "2": ViewAllBooks(); break;
                    case "3": RemoveBook(); break;
                    case "4": AddMember(); break;
                    case "5": ViewAllMembers(); break;
                    case "6": BorrowBook(); break;
                    case "7": ReturnBook(); break;
                    case "8": ViewActiveLoans(); break;
                    case "0": running = false; break;
                    default: Console.WriteLine("Invalid choice."); break;
                }
            }
            catch (Exception ex)
            {
                // Business-rule violations (e.g. borrowing an already-borrowed book)
                // surface here as friendly messages instead of crashing the app.
                Console.WriteLine($"Error: {ex.Message}");
            }

            Console.WriteLine();
        }

        void PrintMenu()
        {
            Console.WriteLine("=== Library Management System ===");
            Console.WriteLine("1. Add book");
            Console.WriteLine("2. View all books");
            Console.WriteLine("3. Remove book");
            Console.WriteLine("4. Add member");
            Console.WriteLine("5. View all members");
            Console.WriteLine("6. Borrow book");
            Console.WriteLine("7. Return book");
            Console.WriteLine("8. View active loans");
            Console.WriteLine("0. Exit");
            Console.Write("Choose an option: ");
        }

        void AddBook()
        {
            Console.Write("Title: ");
            string title = Console.ReadLine() ?? "";

            Console.Write("Author: ");
            string author = Console.ReadLine() ?? "";

            Console.Write("ISBN: ");
            string isbn = Console.ReadLine() ?? "";

            Book book = library.AddBook(title, author, isbn);
            Console.WriteLine($"Added: {book}");
        }

        void ViewAllBooks()
        {
            List<Book> books = library.GetAllBooks();

            if (books.Count == 0)
            {
                Console.WriteLine("No books in the library yet.");
                return;
            }

            foreach (Book b in books)
            {
                Console.WriteLine(b);
            }
        }

        void RemoveBook()
        {
            Console.Write("Book Id to remove: ");
            int id = int.Parse(Console.ReadLine() ?? "0");

            bool removed = library.RemoveBook(id);
            Console.WriteLine(removed ? "Book removed." : "No book found with that Id.");
        }

        void AddMember()
        {
            Console.Write("Name: ");
            string name = Console.ReadLine() ?? "";

            Console.Write("Email: ");
            string email = Console.ReadLine() ?? "";

            Member member = library.AddMember(name, email);
            Console.WriteLine($"Added: {member}");
        }

        void ViewAllMembers()
        {
            List<Member> members = library.GetAllMembers();

            if (members.Count == 0)
            {
                Console.WriteLine("No members registered yet.");
                return;
            }

            foreach (Member m in members)
            {
                Console.WriteLine(m);
            }
        }

        void BorrowBook()
        {
            Console.Write("Book Id: ");
            int bookId = int.Parse(Console.ReadLine() ?? "0");

            Console.Write("Member Id: ");
            int memberId = int.Parse(Console.ReadLine() ?? "0");

            BorrowRecord record = library.BorrowBook(bookId, memberId);
            Console.WriteLine($"Borrowed: {record}");
        }

        void ReturnBook()
        {
            Console.Write("Book Id being returned: ");
            int bookId = int.Parse(Console.ReadLine() ?? "0");

            bool returned = library.ReturnBook(bookId);
            Console.WriteLine(returned ? "Book returned." : "No active loan found for that book.");
        }

        void ViewActiveLoans()
        {
            List<BorrowRecord> loans = library.GetActiveLoans();

            if (loans.Count == 0)
            {
                Console.WriteLine("No books are currently borrowed.");
                return;
            }

            foreach (BorrowRecord r in loans)
            {
                Console.WriteLine(r);
            }
        }
    }
}