namespace PotteryClass.Data.Entities;

public class PeerReviewAssignment
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public Assignment Assignment { get; set; } = null!;
    public Guid ReviewerTeamId { get; set; }
    public AssignmentTeam ReviewerTeam { get; set; } = null!;
    public Guid ReviewedTeamId { get; set; }
    public AssignmentTeam ReviewedTeam { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; }
}
