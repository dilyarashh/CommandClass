namespace PotteryClass.Data.DTOs;

public class UpdatePeerReviewRatingsRequest
{
    public List<PeerReviewRatingRequestDto> Ratings { get; set; } = new();
}

public class PeerReviewRatingRequestDto
{
    public Guid PeerReviewAssignmentId { get; set; }
    public Guid SubmissionId { get; set; }
    public decimal Score { get; set; }
    public string? Comment { get; set; }
}

public class PeerReviewRatingDto
{
    public Guid Id { get; set; }
    public Guid PeerReviewAssignmentId { get; set; }
    public Guid SubmissionId { get; set; }
    public Guid ReviewerUserId { get; set; }
    public Guid ReviewedUserId { get; set; }
    public decimal Score { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}
