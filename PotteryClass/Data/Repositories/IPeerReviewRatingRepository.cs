using PotteryClass.Data.Entities;

namespace PotteryClass.Data.Repositories;

public interface IPeerReviewRatingRepository
{
    Task<List<PeerReviewRating>> GetByReviewerAsync(Guid assignmentId, Guid reviewerUserId);
    Task<List<PeerReviewRating>> GetByReviewerAndAssignmentsAsync(
        Guid assignmentId,
        Guid reviewerUserId,
        IReadOnlyCollection<Guid> peerReviewAssignmentIds);
    Task UpsertAsync(List<PeerReviewRating> ratings);
}
