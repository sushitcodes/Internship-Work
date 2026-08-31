using Form.Entities;
using Microsoft.EntityFrameworkCore;

namespace Form.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<EducationEntry> EducationEntries => Set<EducationEntry>();
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public DbSet<UserRoleAssignment> UserRoleAssignments { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<UserRoleAssignment>()
        .Property(ra => ra.Role)
        .HasConversion<string>();   // same string-storage reasoning as before
}

    

}
