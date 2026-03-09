using Microsoft.EntityFrameworkCore;
using PotteryClass.Data.Entities;

namespace PotteryClass.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<CourseTeacher> CourseTeachers => Set<CourseTeacher>();
    public DbSet<CourseStudent> CourseStudents => Set<CourseStudent>();
    public DbSet<BlackToken> BlackTokens { get; set; } = null!;
    public DbSet<Assignment> Assignments { get; set; }
    public DbSet<AssignmentFile> AssignmentFiles { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<Submission> Submissions { get; set; }
    public DbSet<SubmissionFile> SubmissionFiles { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Course>(b =>
        {
            b.HasKey(x => x.Id);

            b.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            b.Property(x => x.Description)
                .HasMaxLength(2000);

            b.Property(x => x.Code)
                .HasMaxLength(32)
                .IsRequired();

            b.HasIndex(x => x.Code).IsUnique();

            b.Property(x => x.IsActive).IsRequired();
            b.Property(x => x.CreatedByUserId).IsRequired();
            b.Property(x => x.CreatedAtUtc).IsRequired();
        });

        modelBuilder.Entity<CourseTeacher>(b =>
        {
            b.HasKey(x => new { x.CourseId, x.UserId });

            b.HasOne(x => x.Course)
                .WithMany(c => c.Teachers)
                .HasForeignKey(x => x.CourseId);

            b.Property(x => x.CreatedAtUtc).IsRequired();
        });

        modelBuilder.Entity<CourseStudent>(b =>
        {
            b.HasKey(x => new { x.CourseId, x.UserId });

            b.HasOne(x => x.Course)
                .WithMany(c => c.Students)
                .HasForeignKey(x => x.CourseId);

            b.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId);

            b.Property(x => x.IsBlocked).IsRequired();
            b.Property(x => x.CreatedAtUtc).IsRequired();
        });
    }
}