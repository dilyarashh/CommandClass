using Microsoft.EntityFrameworkCore;
using PotteryClass.Data.Entities;
using PotteryClass.Data.Entities.Enums;

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
    public DbSet<AssignmentCaptain> AssignmentCaptains { get; set; }
    public DbSet<AssignmentTeam> AssignmentTeams { get; set; }
    public DbSet<AssignmentTeamMember> AssignmentTeamMembers { get; set; }

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
            b.Property(x => x.RegistrationStartsAtUtc).IsRequired();
            b.Property(x => x.RegistrationEndsAtUtc).IsRequired();
            b.Property(x => x.CreatedByUserId).IsRequired();
            b.Property(x => x.CreatedAtUtc).IsRequired();
        });

        modelBuilder.Entity<CourseTeacher>(b =>
        {
            b.HasKey(x => new { x.CourseId, x.UserId });

            b.HasOne(x => x.Course)
                .WithMany(c => c.Teachers)
                .HasForeignKey(x => x.CourseId);

            b.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId);

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

        modelBuilder.Entity<Assignment>(b =>
        {
            b.HasKey(x => x.Id);

            b.Property(x => x.Title).IsRequired();
            b.Property(x => x.Text).IsRequired();
            b.Property(x => x.TeamFormationMode)
                .HasConversion<int>()
                .IsRequired();
            b.Property(x => x.Created).IsRequired();
            b.Property(x => x.RequiresSubmission).IsRequired();
        });

        modelBuilder.Entity<AssignmentFile>(b =>
        {
            b.HasKey(x => x.Id);

            b.HasOne(x => x.Assignment)
                .WithMany(x => x.Files)
                .HasForeignKey(x => x.AssignmentId);
        });

        modelBuilder.Entity<AssignmentCaptain>(b =>
        {
            b.HasKey(x => new { x.AssignmentId, x.UserId });

            b.Property(x => x.CreatedAtUtc).IsRequired();

            b.HasIndex(x => x.UserId);

            b.HasOne(x => x.Assignment)
                .WithMany(x => x.Captains)
                .HasForeignKey(x => x.AssignmentId);

            b.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId);
        });

        modelBuilder.Entity<Comment>(b =>
        {
            b.HasKey(x => x.Id);

            b.Property(x => x.Text).IsRequired();
            b.Property(x => x.Created).IsRequired();

            b.HasOne(x => x.Assignment)
                .WithMany(x => x.Comments)
                .HasForeignKey(x => x.AssignmentId);

            b.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId);
        });

        modelBuilder.Entity<Submission>(b =>
        {
            b.HasKey(x => x.Id);

            b.Property(x => x.Created).IsRequired();

            b.HasOne(x => x.Assignment)
                .WithMany(x => x.Submissions)
                .HasForeignKey(x => x.AssignmentId);

            b.HasOne(x => x.Student)
                .WithMany()
                .HasForeignKey(x => x.StudentId);
        });

        modelBuilder.Entity<SubmissionFile>(b =>
        {
            b.HasKey(x => x.Id);

            b.HasOne(x => x.Submission)
                .WithMany(x => x.Files)
                .HasForeignKey(x => x.SubmissionId);
        });

        modelBuilder.Entity<AssignmentTeam>(b =>
        {
            b.HasKey(x => x.Id);

            b.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            b.Property(x => x.CreatedAtUtc).IsRequired();

            b.HasIndex(x => new { x.AssignmentId, x.CaptainUserId }).IsUnique();

            b.HasOne(x => x.Assignment)
                .WithMany(x => x.Teams)
                .HasForeignKey(x => x.AssignmentId);

            b.HasOne(x => x.CaptainUser)
                .WithMany()
                .HasForeignKey(x => x.CaptainUserId);
        });

        modelBuilder.Entity<AssignmentTeamMember>(b =>
        {
            b.HasKey(x => new { x.TeamId, x.UserId });

            b.Property(x => x.CreatedAtUtc).IsRequired();

            b.HasOne(x => x.Team)
                .WithMany(x => x.Members)
                .HasForeignKey(x => x.TeamId);

            b.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId);
        });
    }
}
