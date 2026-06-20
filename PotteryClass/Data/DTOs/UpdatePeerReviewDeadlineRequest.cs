using System.Text.Json;

namespace PotteryClass.Data.DTOs;

public class UpdatePeerReviewDeadlineRequest
{
    public JsonElement PeerReviewEndsAtUtc { get; set; }
}

public class PeerReviewDeadlineDto
{
    public Guid AssignmentId { get; set; }
    public DateTime PeerReviewEndsAtUtc { get; set; }
}
