using Form.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace Form.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // old ways to do this same
    // public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<UserRoleAssignment> UserRoleAssignments => Set<UserRoleAssignment>();
    public DbSet<ClassRoom> ClassRooms => Set<ClassRoom>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<Subject> Subjects => Set<Subject>();
    public DbSet<Grade> Grades => Set<Grade>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // UserRoleAssignment.Role is stored as a string in the database
        modelBuilder.Entity<UserRoleAssignment>()
            .Property(ra => ra.Role)
            .HasConversion<string>();

        // Submission configuration — relationship, index, creator link, and soft-delete filter
        modelBuilder.Entity<Submission>(entity =>
        {
            // Relationship — WHERE it points and what happens on delete
            entity.HasOne(s => s.ClassRoom)
                  .WithMany()
                  .HasForeignKey(s => s.ClassRoomId)
                  .OnDelete(DeleteBehavior.Restrict);

            // User gets deleted → submission survives, just loses the creator link
            entity.HasOne<User>()
                  .WithMany()
                  .HasForeignKey(s => s.CreatedByUserId)
                  .OnDelete(DeleteBehavior.SetNull);

            // Index — HOW to look it up fast
            entity.HasIndex(s => s.ClassRoomId);

            // Soft-delete filter: every LINQ query against Submissions automatically
            // excludes rows where IsDeleted = true — no repository method has to
            // remember to add "Where(s => !s.IsDeleted)" itself.
            // To see deleted rows anyway (e.g. an admin recycle bin), use
            // .IgnoreQueryFilters() on the specific query.
            entity.HasQueryFilter(s => !s.IsDeleted);
        });

        modelBuilder.Entity<AttendanceRecord>(entity =>
        {
            // Relationship
            entity.HasOne(a => a.MarkedByUser)
                  .WithMany()
                  .HasForeignKey(a => a.MarkedByUserId)
                  .OnDelete(DeleteBehavior.NoAction);

            // Index
            entity.HasIndex(a => new { a.EnrollmentId, a.Date });
        });
       
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
        //this make the subject with added archieved 0 means active
        modelBuilder.Entity<Subject>().HasQueryFilter(s => !s.IsArchived);
        // it check if there is same name of it or not
        modelBuilder.Entity<Subject>()
    .HasIndex(s => new { s.ClassRoomId, s.Name })
    .IsUnique();

        modelBuilder.Entity<Subject>()
            .HasOne(s => s.ClassRoom)
            .WithMany()
            .HasForeignKey(s => s.ClassRoomId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Grade>()
            .HasOne(g => g.Enrollment)
            .WithMany()
            .HasForeignKey(g => g.EnrollmentId)
            .OnDelete(DeleteBehavior.Cascade);
       
        modelBuilder.Entity<Grade>()
    .HasOne(g => g.Subject)
    .WithMany(s => s.Grades)
    .HasForeignKey(g => g.SubjectId)
    .OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<Grade>()
    .HasOne(g => g.GradedByUser)
    .WithMany()
    .HasForeignKey(g => g.GradedByUserId)
    .OnDelete(DeleteBehavior.NoAction); // same reason as AttendanceRecord.MarkedByUser —
                                        // Grade already cascades through Enrollment → StudentUser,
                                        // a second cascade path through GradedByUser would make
                                        // SQL Server reject the migration outright.

        // "Simple" scope means exactly one grade per student per subject —
        // this index makes that a database-level guarantee, not just something
        // the upsert logic happens to do.
        modelBuilder.Entity<Grade>()
            .HasIndex(g => new { g.EnrollmentId, g.SubjectId })
            .IsUnique();


    }

    // Intercepts every SaveChangesAsync call. Two jobs:
    //   1. Auditing  — auto-set CreatedAt on insert, UpdatedAt on update.
    //   2. Soft delete — convert "Deleted" state into an UPDATE that flips
    //                    IsDeleted instead, so the row survives in the DB.
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // One timestamp per save — every entity touched by this call gets the
        // SAME value, instead of each entity computing DateTime.UtcNow separately
        // and drifting by milliseconds.
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            // Auditing
            if (entry.Entity is IAuditable auditable)
            {
                if (entry.State == EntityState.Added)
                {
                    auditable.CreatedAt = now;
                }
                else if (entry.State == EntityState.Modified)
                {
                    auditable.UpdatedAt = now;
                }
            }

            // Soft delete — intercept the delete BEFORE it reaches the database.
            // Turning a DELETE into an UPDATE means the row is never physically
            // removed; the global query filter (above) hides it from reads.
            if (entry.Entity is ISoftDelete softDeletable && entry.State == EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                softDeletable.IsDeleted = true;
                softDeletable.DeletedAt = now;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }


}