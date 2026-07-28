using BookApi.Models;

namespace Day_9.Services
{
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> GetAllAsync();
        Task<Book?> GetByIdAsync(int id);
        Task<Book> AddAsync(Book book);
        Task<Book?> UpdateAsync(int id, Book book);
        Task<bool> DeleteAsync(int id);
        Task<bool> ExistsAsync(int id);
    }
}