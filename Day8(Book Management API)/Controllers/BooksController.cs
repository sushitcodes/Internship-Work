using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BookMvc.Data;
using BookMvc.Models;
using Microsoft.Extensions.Logging;

namespace BookMvc.Controllers
{
    public class BooksController : Controller
    {
        private readonly BookDbContext _context;
        private readonly ILogger<BooksController> _logger;

        public BooksController(BookDbContext context, ILogger<BooksController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ===== GET: Books =====
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Getting all books for display");
            var books = await _context.Books.ToListAsync();
            return View(books);
        }

        // ===== GET: Books/Details/5 =====
        public async Task<IActionResult> Details(int id)
        {
            _logger.LogInformation("Getting book details for ID: {Id}", id);

            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                _logger.LogWarning("Book with ID {Id} not found", id);
                return NotFound();
            }

            return View(book);
        }

        // ===== GET: Books/Create =====
        public IActionResult Create()
        {
            _logger.LogInformation("Showing create book form");
            return View();
        }

        // ===== POST: Books/Create =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Author,Genre,IsRead,Rating,PublishedYear")] Book book)
        {
            _logger.LogInformation("Creating new book: {Title}", book.Title);

            if (ModelState.IsValid)
            {
                book.AddedDate = DateTime.Now;
                _context.Add(book);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Book created with ID: {Id}", book.Id);
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("Invalid model state for book: {Title}", book.Title);
            return View(book);
        }

        // ===== GET: Books/Edit/5 =====
        public async Task<IActionResult> Edit(int id)
        {
            _logger.LogInformation("Showing edit form for book ID: {Id}", id);

            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                _logger.LogWarning("Book with ID {Id} not found for edit", id);
                return NotFound();
            }

            return View(book);
        }

        // ===== POST: Books/Edit/5 =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Author,Genre,IsRead,Rating,PublishedYear,AddedDate")] Book book)
        {
            if (id != book.Id)
            {
                _logger.LogWarning("ID mismatch in edit: URL {UrlId} vs Model {ModelId}", id, book.Id);
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(book);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Book {Id} updated successfully", id);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookExists(book.Id))
                    {
                        _logger.LogWarning("Book {Id} not found during update", id);
                        return NotFound();
                    }
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            _logger.LogWarning("Invalid model state for editing book {Id}", id);
            return View(book);
        }

        // ===== GET: Books/Delete/5 =====
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogInformation("Showing delete confirmation for book ID: {Id}", id);

            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                _logger.LogWarning("Book with ID {Id} not found for delete", id);
                return NotFound();
            }

            return View(book);
        }

        // ===== POST: Books/Delete/5 =====
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            _logger.LogInformation("Deleting book with ID: {Id}", id);

            var book = await _context.Books.FindAsync(id);

            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Book deleted: {Title} (ID: {Id})", book.Title, book.Id);
            }
            else
            {
                _logger.LogWarning("Book with ID {Id} not found for deletion", id);
            }

            return RedirectToAction(nameof(Index));
        }

        // ===== GET: Books/MarkRead/5 =====
        public async Task<IActionResult> MarkRead(int id)
        {
            _logger.LogInformation("Marking book {Id} as read", id);

            var book = await _context.Books.FindAsync(id);

            if (book == null)
            {
                _logger.LogWarning("Book with ID {Id} not found", id);
                return NotFound();
            }

            book.IsRead = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Book {Id} marked as read", id);
            return RedirectToAction(nameof(Index));
        }

        // ===== GET: Books/Search =====
        public async Task<IActionResult> Search(string searchTerm)
        {
            _logger.LogInformation("Searching for: {SearchTerm}", searchTerm);

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return RedirectToAction(nameof(Index));
            }

            var books = await _context.Books
                .Where(b => b.Title.Contains(searchTerm) || b.Author.Contains(searchTerm))
                .ToListAsync();

            _logger.LogInformation("Found {Count} books matching '{SearchTerm}'", books.Count, searchTerm);
            return View("Index", books);
        }

        private bool BookExists(int id)
        {
            return _context.Books.Any(e => e.Id == id);
        }
    }
}