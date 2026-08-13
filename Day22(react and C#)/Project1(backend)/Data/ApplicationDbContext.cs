using Microsoft.EntityFrameworkCore;
using Project1_backend_.Models;
namespace Project1_backend_.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
         : base(options)
    {
    }

    public DbSet<Submission> Submissions { get; set; }
    public DbSet<Education> Educations { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Education>()
            .HasOne(e => e.Submission)           // Education has ONE Submission
            .WithMany(s => s.Education)          // Submission has MANY Education
            .HasForeignKey(e => e.SubmissionId)  // Foreign key is SubmissionId
            .OnDelete(DeleteBehavior.Cascade);
    }
}