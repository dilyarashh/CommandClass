namespace PotteryClass.Data.DTOs;

public class PeerReviewMyFormDto
{
    public Guid AssignmentId { get; set; }
    public Guid ReviewerTeamId { get; set; }
    public string ReviewerTeamName { get; set; } = null!;
    public DateTime? PeerReviewStartsAtUtc { get; set; }
    public DateTime? PeerReviewEndsAtUtc { get; set; }
    public bool IsReadOnly { get; set; }
    public int TotalCount { get; set; }
    public int CompletedCount { get; set; }
    public int RemainingCount { get; set; }
    public List<PeerReviewFormItemDto> Items { get; set; } = new();
}

public class PeerReviewFormItemDto
{
    public Guid PeerReviewAssignmentId { get; set; }
    public Guid ReviewedTeamId { get; set; }
    public string ReviewedTeamName { get; set; } = null!;
    public List<AssignmentTeamMemberDto> Members { get; set; } = new();
    public List<PeerReviewTeamMemberSubmissionsDto> MemberSubmissions { get; set; } = new();
    public PeerReviewSubmissionDto? FinalSubmission { get; set; }
    public bool IsCompleted { get; set; }
    public decimal? Score { get; set; }
    public string? Comment { get; set; }
}

public class PeerReviewSubmissionDto
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MiddleName { get; set; }
    public DateTime Created { get; set; }
    public bool IsRated { get; set; }
    public decimal? Score { get; set; }
    public string? Comment { get; set; }
    public List<SubmissionFileDto> Files { get; set; } = new();
}

public class PeerReviewTeamMemberSubmissionsDto
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public List<PeerReviewSubmissionDto> Submissions { get; set; } = new();
}
