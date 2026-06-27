using Microsoft.EntityFrameworkCore;
using PotteryClass.Data.Entities;

namespace PotteryClass.Data.Repositories;

public class PeerReviewRatingRepository(AppDbContext db) : IPeerReviewRatingRepository
{
    public Task<List<PeerReviewRating>> GetByReviewerAsync(Guid assignmentId, Guid reviewerUserId)
        => db.PeerReviewRatings
            .Where(x => x.AssignmentId == assignmentId && x.ReviewerUserId == reviewerUserId)
            .ToListAsync();

    public Task<List<PeerReviewRating>> GetByReviewerAndAssignmentsAsync(
        Guid assignmentId,
        Guid reviewerUserId,
        IReadOnlyCollection<Guid> peerReviewAssignmentIds)
        => db.PeerReviewRatings
            .Where(x =>
                x.AssignmentId == assignmentId &&
                x.ReviewerUserId == reviewerUserId &&
                peerReviewAssignmentIds.Contains(x.PeerReviewAssignmentId))
            .ToListAsync();

    public Task<List<PeerReviewRating>> GetByReviewersAndAssignmentsAsync(
        Guid assignmentId,
        IReadOnlyCollection<Guid> reviewerUserIds,
        IReadOnlyCollection<Guid> peerReviewAssignmentIds)
        => db.PeerReviewRatings
            .Include(x => x.ReviewerTeam)
            .Include(x => x.ReviewedTeam)
            .Include(x => x.ReviewerUser)
            .Include(x => x.ReviewedUser)
            .Include(x => x.Submission)
            .Where(x =>
                x.AssignmentId == assignmentId &&
                reviewerUserIds.Contains(x.ReviewerUserId) &&
                peerReviewAssignmentIds.Contains(x.PeerReviewAssignmentId))
            .ToListAsync();

    public async Task UpsertAsync(List<PeerReviewRating> ratings)
    {
        if (ratings.Count == 0)
            return;

        var assignmentId = ratings[0].AssignmentId;
        var reviewerUserId = ratings[0].ReviewerUserId;
        var submissionIds = ratings.Select(x => x.SubmissionId).ToList();

        var existing = await db.PeerReviewRatings
            .Where(x =>
                x.AssignmentId == assignmentId &&
                x.ReviewerUserId == reviewerUserId &&
                submissionIds.Contains(x.SubmissionId))
            .ToListAsync();

        foreach (var rating in ratings)
        {
            var current = existing.FirstOrDefault(x => x.SubmissionId == rating.SubmissionId);
            if (current == null)
            {
                await db.PeerReviewRatings.AddAsync(rating);
                continue;
            }

            current.Score = rating.Score;
            current.Comment = rating.Comment;
            current.UpdatedAtUtc = rating.UpdatedAtUtc;
        }

        await db.SaveChangesAsync();
    }
}
