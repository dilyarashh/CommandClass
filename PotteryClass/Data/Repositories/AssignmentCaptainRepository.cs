using Microsoft.EntityFrameworkCore;
using PotteryClass.Data.Entities;

namespace PotteryClass.Data.Repositories;

public class AssignmentCaptainRepository(AppDbContext db) : IAssignmentCaptainRepository
{
    public Task<List<AssignmentCaptain>> GetByAssignmentAsync(Guid assignmentId)
        => db.Set<AssignmentCaptain>()
            .Include(x => x.User)
            .Where(x => x.AssignmentId == assignmentId)
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync();

    public Task<AssignmentCaptain?> GetAsync(Guid assignmentId, Guid userId)
        => db.Set<AssignmentCaptain>()
            .Include(x => x.User)
            .FirstOrDefaultAsync(x => x.AssignmentId == assignmentId && x.UserId == userId);

    public Task<bool> ExistsAsync(Guid assignmentId, Guid userId)
        => db.Set<AssignmentCaptain>()
            .AnyAsync(x => x.AssignmentId == assignmentId && x.UserId == userId);

    public async Task AddAsync(AssignmentCaptain captain)
        => await db.Set<AssignmentCaptain>().AddAsync(captain);

    public Task RemoveAsync(AssignmentCaptain captain)
    {
        db.Set<AssignmentCaptain>().Remove(captain);
        return Task.CompletedTask;
    }

    public Task SaveChangesAsync()
        => db.SaveChangesAsync();
}
