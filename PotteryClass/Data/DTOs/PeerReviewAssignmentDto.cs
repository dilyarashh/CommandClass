namespace PotteryClass.Data.DTOs;

public class PeerReviewAssignmentDto
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public Guid ReviewerTeamId { get; set; }
    public string ReviewerTeamName { get; set; } = null!;
    public Guid ReviewedTeamId { get; set; }
    public string ReviewedTeamName { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; }
}

public class PeerReviewAssignmentResultDto
{
    public Guid AssignmentId { get; set; }
    public int TeamsCount { get; set; }
    public int RequiredReviewsCount { get; set; }
    public List<PeerReviewAssignmentDto> Assignments { get; set; } = new();
}
