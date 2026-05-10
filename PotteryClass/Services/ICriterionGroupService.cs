using PotteryClass.Data.DTOs;

namespace PotteryClass.Services;

public interface ICriterionGroupService
{
    Task<CriterionGroupDto> CreateAsync(Guid assignmentId, CreateCriterionGroupRequest request);
    Task<List<CriterionGroupDto>> GetByAssignmentAsync(Guid assignmentId);
    Task<CriterionGroupDto> UpdateAsync(Guid criterionGroupId, UpdateCriterionGroupRequest request);
    Task DeleteAsync(Guid criterionGroupId);
}
