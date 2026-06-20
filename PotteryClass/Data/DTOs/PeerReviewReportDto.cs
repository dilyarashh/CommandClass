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
    public int RequiredRatingsCount { get; set; }
    public int ReceivedRatingsCount { get; set; }
    public int MissingRatingsCount { get; set; }
    public bool HasCompletePeerReview { get; set; }
    public bool HasMissingRatings { get; set; }
    public List<PeerReviewReportTeamMemberDto> Members { get; set; } = new();
}

public class PeerReviewReportTeamMemberDto
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public int TotalCount { get; set; }
    public int CompletedCount { get; set; }
    public int RemainingCount { get; set; }
    public string CompletionStatus { get; set; } = null!;
    public bool IsCompleted { get; set; }
}
