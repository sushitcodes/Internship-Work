using Microsoft.EntityFrameworkCore;

public class TodoDbContext : DbContext
{
    public TodoDbContext(DbContextOptions<TodoDbContext> options)
        : base(options)
    {
    }

    public DbSet<Todo> Todos { get; set; }
    // We need this if we doesn't have created on the ssms(it helps) ef to create database on sql
    //protected override void OnModelCreating(ModelBuilder modelBuilder)
    //{
    //    // Optional: Seed initial data
    //    modelBuilder.Entity<Todo>().HasData(
    //       new Todo { Id = 1, Title = "Learn C#", Description = "Complete C# tutorial", IsCompleted = false },
    //       new Todo { Id = 2, Title = "Build API", Description = "Create REST API", IsCompleted = false },
    //       new Todo { Id = 3, Title = "Test Swagger", Description = "Test all endpoints", IsCompleted = true }
    //   );
    //}
    }