namespace PotteryClass.Data.DTOs;

public class PeerReviewPersonalStatusDto
{
    public Guid AssignmentId { get; set; }
    public int TotalCount { get; set; }
    public int CompletedCount { get; set; }
    public int RemainingCount { get; set; }
    public bool IsCompleted { get; set; }
}
