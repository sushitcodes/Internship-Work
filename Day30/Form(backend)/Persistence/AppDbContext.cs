using Form.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace Form.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }
    public DbSet<UserRoleAssignment> UserRoleAssignments { get; set; }
    public DbSet<ClassRoom> ClassRooms { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<AttendanceRecord> AttendanceRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // UserRoleAssignment.Role is stored as a string in the database
        modelBuilder.Entity<UserRoleAssignment>()
            .Property(ra => ra.Role)
            .HasConversion<string>();


        // Prevent dual-cascade-path conflict for AttendanceRecord
        modelBuilder.Entity<AttendanceRecord>()
            .HasOne(a => a.MarkedByUser)
            .WithMany()
            .HasForeignKey(a => a.MarkedByUserId)
            .OnDelete(DeleteBehavior.NoAction);


        // UserProfile configuration
        modelBuilder.Entity<UserProfile>(entity =>
        {
            // One User has one UserProfile
            entity.HasOne(p => p.User)
                  .WithOne(u => u.Profile)
                  .HasForeignKey<UserProfile>(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);


            // UserId must be unique
            entity.HasIndex(p => p.UserId)
                  .IsUnique();


            // MemberNumber is automatically generated
            entity.Property(p => p.MemberNumber)
                  .UseIdentityColumn(seed: 1, increment: 1);


            // Store List<string> as JSON in SQL Server
            entity.Property(p => p.PhoneNumbers)
                  .HasConversion(
                      // C# List<string> -> JSON string
                      list => JsonSerializer.Serialize(
                          list,
                          (JsonSerializerOptions?)null
                      ),

                      // JSON string from database -> C# List<string>
                      json => string.IsNullOrWhiteSpace(json)
                          ? new List<string>()
                          : JsonSerializer.Deserialize<List<string>>(
                              json,
                              (JsonSerializerOptions?)null
                          ) ?? new List<string>()
                  )

                  // Tell EF Core how to compare List<string>
                  .Metadata.SetValueComparer(
                      new ValueComparer<List<string>>(
                          (a, b) => a!.SequenceEqual(b!),

                          v => v.Aggregate(
                              0,
                              (hash, s) =>
                                  HashCode.Combine(
                                      hash,
                                      s.GetHashCode()
                                  )
                          ),

                          v => v.ToList()
                      )
                  );
        });


        // Submission configuration
        modelBuilder.Entity<Submission>(entity =>
        {
            entity.HasOne(s => s.ClassRoom)
                  .WithMany()
                  .HasForeignKey(s => s.ClassRoomId)
                  .OnDelete(DeleteBehavior.Restrict);

            
        });
        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasOne(e => e.StudentUser)
                  .WithMany()
                  .HasForeignKey(e => e.StudentUserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.ClassRoom)
                  .WithMany(c => c.Enrollments)
                  .HasForeignKey(e => e.ClassRoomId)
                  .OnDelete(DeleteBehavior.Cascade);

            // ONE class per student, enforced by the database.
            entity.HasIndex(e => e.StudentUserId).IsUnique();
        });
    }

}