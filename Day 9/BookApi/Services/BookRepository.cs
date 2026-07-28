using BookApi.Data;
using BookApi.Models;
using Microsoft.EntityFrameworkCore;

namespace Day_9.Services
{
    public class BookRepository : IBookRepository
    {
        private readonly AppDbContext _context;
        public BookRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Book>> GetAllAsync()
        {
            return await _context.Books.ToListAsync();
        }

        public async Task<Book?> GetByIdAsync(int id)
        {
            return await _context.Books.FindAsync(id);
        }

        public async Task<Book> AddAsync(Book book)
        {
            book.CreatedAt = DateTime.Now;
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
            return book;
        }

        public async Task<Book?> UpdateAsync(int id, Book book)
        {

            var existingBook = await _context.Books.FindAsync(id);
            if (existingBook == null)
                return null;

            existingBook.Title = book.Title;
            existingBook.Author = book.Author;
            existingBook.Genre = book.Genre;
            existingBook.IsRead = book.IsRead;
            existingBook.Rating = book.Rating;

            await _context.SaveChangesAsync();

            return existingBook;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book == null)
                return false;

            _context.Books.Remove(book);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.Books.AnyAsync(b => b.Id == id);
        }
    }
}