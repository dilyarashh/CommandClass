using PotteryClass.Data.DTOs;

namespace PotteryClass.Services;

public interface ICriterionService
{
    Task<CriterionDto> CreateAsync(Guid criterionGroupId, CreateCriterionRequest request);
    Task<List<CriterionDto>> GetByCriterionGroupAsync(Guid criterionGroupId);
    Task<CriterionDto> UpdateAsync(Guid criterionId, UpdateCriterionRequest request);
    Task DeleteAsync(Guid criterionId);
}
