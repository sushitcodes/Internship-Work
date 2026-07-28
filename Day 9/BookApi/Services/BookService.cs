using BookApi.Models;
using Day_9.Services;

namespace BookApi.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _repository;
        private readonly ILogginService _logger;
        private readonly IEmailService _emailService;
        private readonly ICacheService _cache;


        public BookService(
            IBookRepository repository,
            ILogginService logger,
            IEmailService emailService,
            ICacheService cache)
        {
            _repository = repository;
            _logger = logger;
            _emailService = emailService;
            _cache = cache;
        }

        public async Task<IEnumerable<Book>> GetAllBooksAsync()
        {
            string cacheKey = "all_books";

            var cachedBooks = _cache.Get<IEnumerable<Book>>(cacheKey);

            if (cachedBooks != null)
            {
                _logger.LogInfo("Getting books from cache");
                return cachedBooks;
            }

            _logger.LogInfo("Cache empty. Getting books from database");

            var books = await _repository.GetAllAsync();

            _cache.Set(cacheKey, books);

            return books;
        }

        public async Task<Book?> GetBookByIdAsync(int id)
        {
            string cacheKey = $"book_{id}";

            var cachedBook = _cache.Get<Book>(cacheKey);

            if (cachedBook != null)
            {
                _logger.LogInfo($"Getting book {id} from cache");
                return cachedBook;
            }

            _logger.LogInfo($"Getting book {id} from database");

            var book = await _repository.GetByIdAsync(id);

            if (book != null)
            {
                _cache.Set(cacheKey, book);
            }

            return book;
        }

        public async Task<Book> AddBookAsync(Book book)
        {
            _logger.LogInfo($"Adding new book: {book.Title}");

            book.CreatedAt = DateTime.Now;

            var addedBook = await _repository.AddAsync(book);

            await _emailService.SendEmailAsync(
                "test@test.com",
                "Book Added",
                book.Title);

            _logger.LogInfo($"Book added with ID: {addedBook.Id}");

            return addedBook;
        }

        public async Task<Book?> UpdateBookAsync(int id, Book book)
        {
            _logger.LogInfo($"Updating book with ID: {id}");

            var updatedBook = await _repository.UpdateAsync(id, book);

            if (updatedBook != null)
            {
                _logger.LogInfo($"Book updated successfully: {id}");
            }

            return updatedBook;
        }

        public async Task<bool> DeleteBookAsync(int id)
        {
            _logger.LogInfo($"Deleting book with ID: {id}");

            var deleted = await _repository.DeleteAsync(id);

            if (deleted)
            {
                _logger.LogInfo($"Book deleted successfully: {id}");
            }

            return deleted;
        }

        public async Task<bool> BookExistsAsync(int id)
        {
            return await _repository.ExistsAsync(id);
        }
    }
}