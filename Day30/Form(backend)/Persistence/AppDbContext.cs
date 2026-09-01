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
    public DbSet<AttendanceRecord> AttendanceRecords { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<UserRoleAssignment>()
        .Property(ra => ra.Role)
        .HasConversion<string>();

        modelBuilder.Entity<AttendanceRecord>()   
        .Property(a => a.Status)
        .HasConversion<string>();

        //break the dual - cascade - path conflict
    modelBuilder.Entity<AttendanceRecord>()
        .HasOne(a => a.MarkedByUser)
        .WithMany()
        .HasForeignKey(a => a.MarkedByUserId)
        .OnDelete(DeleteBehavior.NoAction);
    }

    

}
