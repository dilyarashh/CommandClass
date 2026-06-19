using Microsoft.EntityFrameworkCore;
using PotteryClass.Data.Entities;

namespace PotteryClass.Data.Repositories;

public class PeerReviewAssignmentRepository(AppDbContext db) : IPeerReviewAssignmentRepository
{
    public Task<List<PeerReviewAssignment>> GetByAssignmentAsync(Guid assignmentId)
        => db.PeerReviewAssignments
            .Include(x => x.ReviewerTeam)
            .Include(x => x.ReviewedTeam)
            .Where(x => x.AssignmentId == assignmentId)
            .OrderBy(x => x.ReviewerTeam.CreatedAtUtc)
            .ThenBy(x => x.ReviewedTeam.CreatedAtUtc)
            .ToListAsync();

    public Task<List<PeerReviewAssignment>> GetByReviewerTeamAsync(Guid assignmentId, Guid reviewerTeamId)
        => db.PeerReviewAssignments
            .Include(x => x.ReviewerTeam)
            .Include(x => x.ReviewedTeam)
            .ThenInclude(x => x.Members)
            .ThenInclude(x => x.User)
            .Include(x => x.ReviewedTeam)
            .ThenInclude(x => x.FinalSubmission)
            .ThenInclude(x => x!.Files)
            .Include(x => x.ReviewedTeam)
            .ThenInclude(x => x.FinalSubmission)
            .ThenInclude(x => x!.Student)
            .Where(x => x.AssignmentId == assignmentId && x.ReviewerTeamId == reviewerTeamId)
            .OrderBy(x => x.ReviewedTeam.CreatedAtUtc)
            .ToListAsync();

    public async Task ReplaceForAssignmentAsync(Guid assignmentId, List<PeerReviewAssignment> assignments)
    {
        var existing = await db.PeerReviewAssignments
            .Where(x => x.AssignmentId == assignmentId)
            .ToListAsync();

        db.PeerReviewAssignments.RemoveRange(existing);
        await db.PeerReviewAssignments.AddRangeAsync(assignments);
        await db.SaveChangesAsync();
    }
}
