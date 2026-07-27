using BookApi.Models;
using BookApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BookApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;
        private readonly ILogger<BooksController> _logger;

        // DEPENDENCY INJECTION
        // "Give me the service I need" - this connects the Controller to the Service
        public BooksController(IBookService bookService, ILogger<BooksController> logger)
        {
            _bookService = bookService;
            _logger = logger;
        }

        // GET: api/books
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Book>>> GetBooks()
        {
            _logger.LogInformation("GET /api/books - Getting all books");
            var books = await _bookService.GetAllBooksAsync();
            return Ok(books);
        }

        // GET: api/books/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Book>> GetBook(int id)
        {
            _logger.LogInformation("GET /api/books/{Id} - Getting book", id);

            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
            {
                _logger.LogWarning("Book with ID {Id} not found", id);
                return NotFound($"Book with ID {id} not found");
            }

            return Ok(book);
        }

        // POST: api/books
        [HttpPost]
        public async Task<ActionResult<Book>> PostBook(Book book)
        {
            _logger.LogInformation("POST /api/books - Creating book: {Title}", book.Title);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for book: {Title}", book.Title);
                return BadRequest(ModelState);
            }

            var createdBook = await _bookService.AddBookAsync(book);
            return CreatedAtAction(nameof(GetBook), new { id = createdBook.Id }, createdBook);
        }

        // PUT: api/books/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBook(int id, Book book)
        {
            _logger.LogInformation("PUT /api/books/{Id} - Updating book", id);

            if (id != book.Id)
            {
                _logger.LogWarning("ID mismatch: URL {UrlId} vs Body {BodyId}", id, book.Id);
                return BadRequest("ID mismatch");
            }

            var updatedBook = await _bookService.UpdateBookAsync(id, book);
            if (updatedBook == null)
            {
                _logger.LogWarning("Book with ID {Id} not found", id);
                return NotFound($"Book with ID {id} not found");
            }

            return Ok(updatedBook);
        }

        // DELETE: api/books/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            _logger.LogInformation("DELETE /api/books/{Id} - Deleting book", id);

            var deleted = await _bookService.DeleteBookAsync(id);
            if (!deleted)
            {
                _logger.LogWarning("Book with ID {Id} not found", id);
                return NotFound($"Book with ID {id} not found");
            }

            return NoContent();
        }
    }
}