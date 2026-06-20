namespace PotteryClass.Data.DTOs;

public class PeerReviewTeamStatusDto
{
    public Guid AssignmentId { get; set; }
    public Guid TeamId { get; set; }
    public string TeamName { get; set; } = null!;
    public DateTime? PeerReviewStartsAtUtc { get; set; }
    public DateTime? PeerReviewEndsAtUtc { get; set; }
    public int RequiredReviewsCount { get; set; }
    public int MembersCount { get; set; }
    public int CompletedMembersCount { get; set; }
    public int RemainingMembersCount { get; set; }
    public bool IsCompleted { get; set; }
    public PeerReviewTeamMemberStatusDto CurrentStudent { get; set; } = null!;
    public List<PeerReviewTeamMemberStatusDto> Members { get; set; } = new();
}

public class PeerReviewTeamMemberStatusDto
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public int TotalCount { get; set; }
    public int CompletedCount { get; set; }
    public int RemainingCount { get; set; }
    public bool IsCompleted { get; set; }
}
