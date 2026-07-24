using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {

    }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // This adds test data when database is created
        modelBuilder.Entity<User>().HasData

(
        new User { Id = 1, Name = "Sushit Chaulagain", Email = "Sushit1@hgmail.com", Age = 25 },
    new User { Id = 2, Name = "Roshan Dahal", Email = "Roshan1@hgmail.com", Age = 25 },
    new User { Id = 3, Name = "Saditya Adhikari", Email = "Saditya@hgmail.com", Age = 26 }
    );
    }
}