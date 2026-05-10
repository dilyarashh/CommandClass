using Microsoft.EntityFrameworkCore;
using PotteryClass.Data.Entities;

namespace PotteryClass.Data.Repositories;

public class CriterionGroupRepository(AppDbContext db) : ICriterionGroupRepository
{
    public Task<Assignment?> GetAssignmentAsync(Guid assignmentId)
        => db.Assignments.FirstOrDefaultAsync(x => x.Id == assignmentId);

    public Task<CriterionGroup?> GetByIdAsync(Guid criterionGroupId)
        => db.CriterionGroups.FirstOrDefaultAsync(x => x.Id == criterionGroupId);

    public Task<List<CriterionGroup>> GetByAssignmentIdAsync(Guid assignmentId)
        => db.CriterionGroups
            .Where(x => x.AssignmentId == assignmentId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedAtUtc)
            .ToListAsync();

    public async Task AddAsync(CriterionGroup criterionGroup)
        => await db.CriterionGroups.AddAsync(criterionGroup);

    public void Delete(CriterionGroup criterionGroup)
        => db.CriterionGroups.Remove(criterionGroup);

    public Task SaveChangesAsync()
        => db.SaveChangesAsync();
}
