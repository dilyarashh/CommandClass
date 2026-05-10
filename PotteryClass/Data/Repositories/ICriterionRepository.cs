using PotteryClass.Data.Entities;

namespace PotteryClass.Data.Repositories;

public interface ICriterionRepository
{
    Task<CriterionGroup?> GetCriterionGroupAsync(Guid criterionGroupId);
    Task<Criterion?> GetByIdAsync(Guid criterionId);
    Task<List<Criterion>> GetByCriterionGroupIdAsync(Guid criterionGroupId);
    Task AddAsync(Criterion criterion);
    void Delete(Criterion criterion);
    Task SaveChangesAsync();
}
