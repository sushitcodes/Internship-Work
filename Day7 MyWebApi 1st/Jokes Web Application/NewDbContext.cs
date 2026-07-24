using Microsoft.EntityFrameworkCore;

public class NewDbContext(DbContextOptions<NewDbContext> options) : DbContext(options)
{
    public DbSet<Joke> Joke { get; set; } = default!;
}
