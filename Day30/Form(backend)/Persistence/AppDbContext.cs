using Form.Entities;
using Microsoft.AspNetCore.Http.HttpResults;
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
    public DbSet<ClassRoom> ClassRooms { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<AttendanceRecord> AttendanceRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)

{
    modelBuilder.Entity<UserRoleAssignment>()
        .Property(ra => ra.Role)
        .HasConversion<string>();

        //break the dual - cascade - path conflict
    modelBuilder.Entity<AttendanceRecord>()
        .HasOne(a => a.MarkedByUser)
        .WithMany()
        .HasForeignKey(a => a.MarkedByUserId)
        .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasOne(p => p.User)
                  .WithOne()
                  .HasForeignKey<UserProfile>(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(p => p.UserId).IsUnique();

            // This is the actual auto-increment instruction. Without it, EF Core
            // would treat MemberNumber as a normal int column (defaulting to 0,
            // and you'd have to set it yourself in code — the opposite of what we want).
            entity.Property(p => p.MemberNumber)
                  .UseIdentityColumn(seed: 1, increment: 1);
        });
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasOne(p => p.User)
                  .WithOne(u => u.Profile) // ← now points at the actual nav property you just added
                  .HasForeignKey<UserProfile>(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(p => p.UserId).IsUnique();

            entity.Property(p => p.MemberNumber)
                  .UseIdentityColumn(seed: 1, increment: 1);
        });
    }

    

}
