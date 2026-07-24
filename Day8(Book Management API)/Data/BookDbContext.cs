using Microsoft.EntityFrameworkCore;
using BookMvc.Models;

namespace BookMvc.Data
{
    public class BookDbContext : DbContext
    {
        public BookDbContext(DbContextOptions<BookDbContext> options)
            : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed data
            modelBuilder.Entity<Book>().HasData(
                new Book
                {
                    Id = 1,
                    Title = "The Great Gatsby",
                    Author = "F. Scott Fitzgerald",
                    Genre = "Fiction",
                    IsRead = true,
                    Rating = 5,
                    PublishedYear = 1925
                },
                new Book
                {
                    Id = 2,
                    Title = "C# Programming",
                    Author = "John Doe",
                    Genre = "Technical",
                    IsRead = false,
                    Rating = 0,
                    PublishedYear = 2020
                },
                new Book
                {
                    Id = 3,
                    Title = "Clean Code",
                    Author = "Robert C. Martin",
                    Genre = "Technical",
                    IsRead = true,
                    Rating = 5,
                    PublishedYear = 2008
                }
            );
        }
    }
}