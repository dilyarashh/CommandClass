namespace PotteryClass.Data.DTOs;

public class PeerReviewReportDto
{
    public Guid AssignmentId { get; set; }
    public DateTime? PeerReviewStartsAtUtc { get; set; }
    public DateTime? PeerReviewEndsAtUtc { get; set; }
    public int TeamsCount { get; set; }
    public List<PeerReviewReportTeamDto> Teams { get; set; } = new();
}

public class PeerReviewReportTeamDto
{
    public Guid TeamId { get; set; }
    public string TeamName { get; set; } = null!;
    public int MembersCount { get; set; }
    public int CompletedMembersCount { get; set; }
    public int RemainingMembersCount { get; set; }
    public bool IsCompleted { get; set; }
}
