using BookTracker.Data;
using BookTracker.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

class Program
{
    static void Main(string[] args)
    {
        //Building the Configuration
        var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        //Get the connection string we put the key here
        string connectionString = config.GetConnectionString("DefaultConnection");
        //Building  DbCOntext Option
        var options = new DbContextOptionsBuilder<BookContext>()
            .UseSqlServer(connectionString)
            .Options;

        //Create BookContext
        using var context = new BookContext(options);
        //Create a database one shot
        Console.WriteLine("Creating database...");
        context.Database.EnsureCreated();
        Console.WriteLine("Database created!");

        //Create a new book for it 
        if (!context.Books.Any())
        {

            var books = new List<Book>
            {
    new Book {
        Title = "One Piece",
        Author = "Oda",
        IsRead = true
    },
    new Book {
        Title = "Black Clover",
        Author = "Asta",
        IsRead = false
    }
        }  ;
        // Stage the book
        context.Books.AddRange(books);

        // Commit to database
        context.SaveChanges();
    }

        //Reading the books
        var allbooks = context.Books.ToList();
        Console.WriteLine($"{allbooks.Count} books found:");
        foreach (var book in allbooks)
        {
            Console.WriteLine($"Title: {book.Title}, Author: {book.Author}, IsRead: {book.IsRead}");
        }

        //FInd the book by title to update
        var onePiece = context.Books.FirstOrDefault(b => b.Title == "One Piece");
        if (onePiece != null)
        {
            onePiece.IsRead = false;
            context.SaveChanges();
            Console.WriteLine($"Updated '{onePiece.Title}' → IsRead = {onePiece.IsRead}");


        }
        else
        {
            Console.WriteLine("Book not Found'One Piece'");
        }
        var onePieceRemove = context.Books.FirstOrDefault(b => b.Title == "One Piece");
        if (onePieceRemove != null)
        {
            context.Books.Remove(onePieceRemove);
            context.SaveChanges();
            Console.WriteLine($"Deleted book: '{onePieceRemove.Title}' by {onePieceRemove.Author}");
        }
        else
        {
            Console.WriteLine("Book Not Found");
        }

        //Check it the book is created 

    }
}