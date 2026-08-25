using Form.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Form.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<EducationEntry> EducationEntries => Set<EducationEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Submission>()
            .HasMany(s => s.Education)
            .WithOne(e => e.Submission)
            .HasForeignKey(e => e.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Submission>().Property(s => s.FullName).HasMaxLength(50).IsRequired();
        modelBuilder.Entity<Submission>().Property(s => s.Email).HasMaxLength(100).IsRequired();
        modelBuilder.Entity<Submission>().Property(s => s.Phone).HasMaxLength(20).IsRequired();
        modelBuilder.Entity<EducationEntry>().Property(e => e.Institution).HasMaxLength(100).IsRequired();
        modelBuilder.Entity<EducationEntry>().Property(e => e.Degree).HasMaxLength(100).IsRequired();
    }
}
