using PotteryClass.Data.Entities;

namespace PotteryClass.Data.Repositories;

public interface ICriterionGroupRepository
{
    Task<Assignment?> GetAssignmentAsync(Guid assignmentId);
    Task<CriterionGroup?> GetByIdAsync(Guid criterionGroupId);
    Task<List<CriterionGroup>> GetByAssignmentIdAsync(Guid assignmentId);
    Task AddAsync(CriterionGroup criterionGroup);
    void Delete(CriterionGroup criterionGroup);
    Task SaveChangesAsync();
}
