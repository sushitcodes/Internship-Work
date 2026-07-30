using BookTracker.Data;
using BookTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace BookTracker.Tests
{
    [TestClass]
    public sealed class BookContextTests
    {
        [TestMethod]
        public void AddBook_ShouldIncreaseBookCount()
        {//Arrange
            var options = new DbContextOptionsBuilder<BookContext>()
                .UseInMemoryDatabase("AddBookTestDb")
                .Options;

            using var context = new BookContext(options);
            var book = new Book
            {
                Title = "Test Book",
                Author = "Test Author",
                IsRead = false
            };
            //Act
            context.Books.Add(book);
            context.SaveChanges();
            //Assert
            Assert.AreEqual(1, context.Books.Count());
        }

    }

}

