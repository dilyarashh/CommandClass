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
    public DbSet<Criterion> Criteria { get; set; }
    public DbSet<CriterionGroup> CriterionGroups { get; set; }
    public DbSet<Submission> Submissions { get; set; }
    public DbSet<SubmissionAssessment> SubmissionAssessments { get; set; }
    public DbSet<SubmissionFile> SubmissionFiles { get; set; }
    public DbSet<AssignmentCaptain> AssignmentCaptains { get; set; }
    public DbSet<AssignmentTeam> AssignmentTeams { get; set; }
    public DbSet<AssignmentTeamMember> AssignmentTeamMembers { get; set; }
    public DbSet<PeerReviewAssignment> PeerReviewAssignments { get; set; }
    public DbSet<PeerReviewRating> PeerReviewRatings { get; set; }

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
            b.Property(x => x.GradingRules);
            b.Property(x => x.Created).IsRequired();
            b.Property(x => x.IsVisible).IsRequired();
            b.Property(x => x.RequiresSubmission).IsRequired();
            b.Property(x => x.PeerReviewEnabled).IsRequired();
            b.Property(x => x.PeerReviewPenaltyPercent)
                .HasPrecision(5, 2)
                .IsRequired();

            b.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.DraftCurrentCaptainUserId);
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

        modelBuilder.Entity<CriterionGroup>(b =>
        {
            b.HasKey(x => x.Id);

            b.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            b.Property(x => x.Description)
                .HasMaxLength(2000);

            b.Property(x => x.SortOrder).IsRequired();
            b.Property(x => x.CreatedAtUtc).IsRequired();

            b.HasOne(x => x.Assignment)
                .WithMany(x => x.CriterionGroups)
                .HasForeignKey(x => x.AssignmentId);

            b.HasIndex(x => x.AssignmentId);
        });

        modelBuilder.Entity<Criterion>(b =>
        {
            b.HasKey(x => x.Id);

            b.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            b.Property(x => x.Description)
                .HasMaxLength(2000);

            b.Property(x => x.Type)
                .HasMaxLength(64)
                .IsRequired();

            b.Property(x => x.Category)
                .HasMaxLength(64)
                .IsRequired();

            b.Property(x => x.Settings)
                .IsRequired();

            b.Property(x => x.MaxScore).IsRequired();
            b.Property(x => x.SortOrder).IsRequired();
            b.Property(x => x.CreatedAtUtc).IsRequired();

            b.HasOne(x => x.CriterionGroup)
                .WithMany(x => x.Criteria)
                .HasForeignKey(x => x.CriterionGroupId);

            b.HasIndex(x => x.CriterionGroupId);
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

        modelBuilder.Entity<SubmissionAssessment>(b =>
        {
            b.HasKey(x => x.Id);

            b.Property(x => x.CriterionValues).IsRequired();
            b.Property(x => x.MainPoints).IsRequired();
            b.Property(x => x.BonusPoints).IsRequired();
            b.Property(x => x.PenaltyPoints).IsRequired();
            b.Property(x => x.Multiplier).IsRequired();
            b.Property(x => x.FinalGrade).IsRequired();
            b.Property(x => x.CalculationDetails).IsRequired();
            b.Property(x => x.CheckedAtUtc).IsRequired();
            b.Property(x => x.Comment).HasMaxLength(4000);

            b.HasIndex(x => x.SubmissionId).IsUnique();
            b.HasIndex(x => x.AssignmentId);
            b.HasIndex(x => x.StudentId);
            b.HasIndex(x => x.CheckedByUserId);

            b.HasOne(x => x.Submission)
                .WithOne(x => x.Assessment)
                .HasForeignKey<SubmissionAssessment>(x => x.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne<Assignment>()
                .WithMany()
                .HasForeignKey(x => x.AssignmentId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne<User>()
                .WithMany()
                .HasForeignKey(x => x.CheckedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
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

            b.HasOne(x => x.FinalSubmission)
                .WithMany()
                .HasForeignKey(x => x.FinalSubmissionId)
                .OnDelete(DeleteBehavior.SetNull);
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

        modelBuilder.Entity<PeerReviewAssignment>(b =>
        {
            b.HasKey(x => x.Id);

            b.Property(x => x.CreatedAtUtc).IsRequired();

            b.HasIndex(x => x.AssignmentId);
            b.HasIndex(x => x.ReviewerTeamId);
            b.HasIndex(x => x.ReviewedTeamId);
            b.HasIndex(x => new { x.AssignmentId, x.ReviewerTeamId, x.ReviewedTeamId }).IsUnique();

            b.HasOne(x => x.Assignment)
                .WithMany(x => x.PeerReviewAssignments)
                .HasForeignKey(x => x.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.ReviewerTeam)
                .WithMany()
                .HasForeignKey(x => x.ReviewerTeamId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.ReviewedTeam)
                .WithMany()
                .HasForeignKey(x => x.ReviewedTeamId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PeerReviewRating>(b =>
        {
            b.HasKey(x => x.Id);

            b.Property(x => x.Score)
                .HasPrecision(7, 2)
                .IsRequired();

            b.Property(x => x.Comment).HasMaxLength(4000);
            b.Property(x => x.CreatedAtUtc).IsRequired();
            b.Property(x => x.UpdatedAtUtc).IsRequired();

            b.HasIndex(x => x.AssignmentId);
            b.HasIndex(x => x.PeerReviewAssignmentId);
            b.HasIndex(x => x.ReviewerUserId);
            b.HasIndex(x => x.ReviewedUserId);
            b.HasIndex(x => x.SubmissionId);
            b.HasIndex(x => new { x.PeerReviewAssignmentId, x.ReviewerUserId, x.SubmissionId }).IsUnique();

            b.HasOne(x => x.Assignment)
                .WithMany(x => x.PeerReviewRatings)
                .HasForeignKey(x => x.AssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.PeerReviewAssignment)
                .WithMany()
                .HasForeignKey(x => x.PeerReviewAssignmentId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(x => x.ReviewerTeam)
                .WithMany()
                .HasForeignKey(x => x.ReviewerTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.ReviewedTeam)
                .WithMany()
                .HasForeignKey(x => x.ReviewedTeamId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.ReviewerUser)
                .WithMany()
                .HasForeignKey(x => x.ReviewerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.ReviewedUser)
                .WithMany()
                .HasForeignKey(x => x.ReviewedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(x => x.Submission)
                .WithMany()
                .HasForeignKey(x => x.SubmissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
