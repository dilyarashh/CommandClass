namespace PotteryClass.Data.Entities;

public class PeerReviewRating
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public Assignment Assignment { get; set; } = null!;
    public Guid PeerReviewAssignmentId { get; set; }
    public PeerReviewAssignment PeerReviewAssignment { get; set; } = null!;
    public Guid ReviewerTeamId { get; set; }
    public AssignmentTeam ReviewerTeam { get; set; } = null!;
    public Guid ReviewedTeamId { get; set; }
    public AssignmentTeam ReviewedTeam { get; set; } = null!;
    public Guid ReviewerUserId { get; set; }
    public User ReviewerUser { get; set; } = null!;
    public Guid ReviewedUserId { get; set; }
    public User ReviewedUser { get; set; } = null!;
    public Guid SubmissionId { get; set; }
    public Submission Submission { get; set; } = null!;
    public decimal Score { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
