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
