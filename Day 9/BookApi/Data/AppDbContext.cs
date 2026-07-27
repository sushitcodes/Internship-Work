using BookApi.Models;
using Microsoft.EntityFrameworkCore;

namespace BookApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
                    : base(options)
        {
        }
        public DbSet<Book> Books { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed sample data
            modelBuilder.Entity<Book>().HasData(
                new Book
                {
                    Id = 1,
                    Title = "Clean Code",
                    Author = "Robert C. Martin",
                    Genre = "Technical",
                    IsRead = true,
                    Rating = 5
                },
                new Book
                {
                    Id = 2,
                    Title = "The Pragmatic Programmer",
                    Author = "Andrew Hunt",
                    Genre = "Technical",
                    IsRead = false,
                    Rating = 0
                },
                new Book
                {
                    Id = 3,
                    Title = "C# In Depth",
                    Author = "Jon Skeet",
                    Genre = "Technical",
                    IsRead = true,
                    Rating = 4.5
                }
            );
        }
    }


}
