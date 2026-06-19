namespace PotteryClass.Data.DTOs;

public class UpdateAssignmentPeerReviewRequest
{
    public bool PeerReviewEnabled { get; set; }

    public DateTime? PeerReviewStartsAtUtc { get; set; }

    public DateTime? PeerReviewEndsAtUtc { get; set; }

    public int? PeerReviewRequiredReviewsCount { get; set; }

    public decimal? PeerReviewPenaltyPercent { get; set; }
}
