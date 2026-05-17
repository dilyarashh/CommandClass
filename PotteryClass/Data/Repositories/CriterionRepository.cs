using Microsoft.EntityFrameworkCore;
using PotteryClass.Data.Entities;

namespace PotteryClass.Data.Repositories;

public class CriterionRepository(AppDbContext db) : ICriterionRepository
{
    public Task<CriterionGroup?> GetCriterionGroupAsync(Guid criterionGroupId)
        => db.CriterionGroups
            .Include(x => x.Assignment)
            .FirstOrDefaultAsync(x => x.Id == criterionGroupId);

    public Task<Criterion?> GetByIdAsync(Guid criterionId)
        => db.Criteria
            .Include(x => x.CriterionGroup)
            .ThenInclude(x => x.Assignment)
            .FirstOrDefaultAsync(x => x.Id == criterionId);

    public Task<List<Criterion>> GetByCriterionGroupIdAsync(Guid criterionGroupId)
        => db.Criteria
            .Where(x => x.CriterionGroupId == criterionGroupId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedAtUtc)
            .ToListAsync();

    public Task<List<Criterion>> GetByAssignmentIdAsync(Guid assignmentId)
        => db.Criteria
            .Include(x => x.CriterionGroup)
            .Where(x => x.CriterionGroup.AssignmentId == assignmentId)
            .OrderBy(x => x.CriterionGroup.SortOrder)
            .ThenBy(x => x.SortOrder)
            .ThenBy(x => x.CreatedAtUtc)
            .ToListAsync();

    public async Task AddAsync(Criterion criterion)
        => await db.Criteria.AddAsync(criterion);

    public void Delete(Criterion criterion)
        => db.Criteria.Remove(criterion);

    public Task SaveChangesAsync()
        => db.SaveChangesAsync();
}
