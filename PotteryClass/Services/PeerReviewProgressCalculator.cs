using PotteryClass.Data.Entities;

namespace PotteryClass.Services;

public static class PeerReviewProgressCalculator
{
    public static int CountCompletedPeerReviewAssignments(
        Guid reviewerUserId,
        IReadOnlyCollection<PeerReviewAssignment> peerReviewAssignments,
        IReadOnlyDictionary<Guid, List<Submission>> reviewedSubmissionsByAssignmentId,
        IReadOnlySet<(Guid ReviewerUserId, Guid PeerReviewAssignmentId, Guid SubmissionId)> ratedKeys)
        => peerReviewAssignments.Count(peerReviewAssignment =>
        {
            var submissions = reviewedSubmissionsByAssignmentId[peerReviewAssignment.Id];

            return submissions.Count > 0 &&
                   submissions.All(submission => ratedKeys.Contains((
                       reviewerUserId,
                       peerReviewAssignment.Id,
                       submission.Id)));
        });
}
