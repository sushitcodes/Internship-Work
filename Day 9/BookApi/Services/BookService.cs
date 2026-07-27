using BookApi.Data;
using BookApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BookApi.Services
{
    public class BookService : IBookService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BookService> _logger;


        public BookService(AppDbContext context, ILogger<BookService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<Book>> GetAllBooksAsync()
        {
            _logger.LogInformation("Getting all books from database");
            return await _context.Books.ToListAsync();
        }

        public async Task<Book> GetBookByIdAsync(int id)
        {
            _logger.LogInformation("Getting book with ID: {Id}", id);
            return await _context.Books.FindAsync(id);
        }

        public async Task<Book> AddBookAsync(Book book)
        {
            _logger.LogInformation("Adding new book: {Title}", book.Title);

            book.CreatedAt = DateTime.Now;
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Book added with ID: {Id}", book.Id);
            return book;
        }

        public async Task<Book> UpdateBookAsync(int id, Book book)
        {
            _logger.LogInformation("Updating book with ID: {Id}", id);

            var existingBook = await _context.Books.FindAsync(id);
            if (existingBook == null)
                return null;

            existingBook.Title = book.Title;
            existingBook.Author = book.Author;
            existingBook.Genre = book.Genre;
            existingBook.IsRead = book.IsRead;
            existingBook.Rating = book.Rating;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Book {Id} updated successfully", id);

            return existingBook;
        }

        public async Task<bool> DeleteBookAsync(int id)
        {
            _logger.LogInformation("Deleting book with ID: {Id}", id);

            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return false;

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Book {Id} deleted successfully", id);
            return true;
        }

        public async Task<bool> BookExistsAsync(int id)
        {
            return await _context.Books.AnyAsync(b => b.Id == id);
        }
    }
}
