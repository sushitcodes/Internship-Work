using BookApi.Controllers;
using BookApi.Data;
using BookApi.Models;
using BookApi.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace SimpleBookApi.Tests
{
    [TestFixture]
    public class BookControllerTests
    {
        private AppDbContext _context;
        private BookService _service;
        private BooksController _controller;

        [SetUp]
        public void Setup()
        {
            // Create in-memory database for testing
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;

            _context = new AppDbContext(options);

            // Seed test data
            _context.Books.AddRange(
                new Book { Id = 1, Title = "Test Book 1", Author = "Author 1", Rating = 4 },
                new Book { Id = 2, Title = "Test Book 2", Author = "Author 2", Rating = 5 }
            );
            _context.SaveChanges();

            // Create service with mocked logger
            var loggerMock = new Mock<ILogger<BookService>>();
            _service = new BookService(_context, loggerMock.Object);

            // Create controller with mocked logger
            var controllerLoggerMock = new Mock<ILogger<BooksController>>();
            _controller = new BooksController(_service, controllerLoggerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        // TEST 1: GET all books returns all
        [Test]
        public async Task GetBooks_ReturnsAllBooks()
        {
            // Act
            var result = await _controller.GetBooks();

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var books = okResult.Value as List<Book>;
            Assert.AreEqual(2, books.Count);
        }

        // TEST 2: GET by ID returns correct book
        [Test]
        public async Task GetBook_ValidId_ReturnsBook()
        {
            // Act
            var result = await _controller.GetBook(1);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);

            var book = okResult.Value as Book;
            Assert.AreEqual(1, book.Id);
            Assert.AreEqual("Test Book 1", book.Title);
        }

        // TEST 3: GET by ID with invalid ID returns 404
        [Test]
        public async Task GetBook_InvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.GetBook(999);

            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }

        // TEST 4: POST creates new book
        [Test]
        public async Task PostBook_ValidBook_CreatesBook()
        {
            // Arrange
            var newBook = new Book
            {
                Title = "New Test Book",
                Author = "New Author",
                Genre = "Fiction",
                IsRead = false,
                Rating = 3
            };

            // Act
            var result = await _controller.PostBook(newBook);

            // Assert
            var createdResult = result.Result as CreatedAtActionResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual(201, createdResult.StatusCode);

            var book = createdResult.Value as Book;
            Assert.AreEqual("New Test Book", book.Title);

            // Verify it was added to database
            var count = await _context.Books.CountAsync();
            Assert.AreEqual(3, count);
        }

        // TEST 5: DELETE removes book
        [Test]
        public async Task DeleteBook_ValidId_DeletesBook()
        {
            // Act
            var result = await _controller.DeleteBook(1);

            // Assert
            Assert.IsInstanceOf<NoContentResult>(result);

            // Verify it was deleted
            var deleted = await _context.Books.FindAsync(1);
            Assert.IsNull(deleted);
        }

        // TEST 6: DELETE with invalid ID returns 404
        [Test]
        public async Task DeleteBook_InvalidId_ReturnsNotFound()
        {
            // Act
            var result = await _controller.DeleteBook(999);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.IsNotNull(notFoundResult);
            Assert.AreEqual(404, notFoundResult.StatusCode);
        }
    }
}