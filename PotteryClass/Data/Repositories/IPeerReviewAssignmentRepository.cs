using PotteryClass.Data.Entities;

namespace PotteryClass.Data.Repositories;

public interface IPeerReviewAssignmentRepository
{
    Task<List<PeerReviewAssignment>> GetByAssignmentAsync(Guid assignmentId);
    Task<List<PeerReviewAssignment>> GetByReviewerTeamAsync(Guid assignmentId, Guid reviewerTeamId);
    Task ReplaceForAssignmentAsync(Guid assignmentId, List<PeerReviewAssignment> assignments);
}
