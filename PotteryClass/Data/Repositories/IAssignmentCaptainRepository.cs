using PotteryClass.Data.Entities;

namespace PotteryClass.Data.Repositories;

public interface IAssignmentCaptainRepository
{
    Task<List<AssignmentCaptain>> GetByAssignmentAsync(Guid assignmentId);
    Task<AssignmentCaptain?> GetAsync(Guid assignmentId, Guid userId);
    Task<bool> ExistsAsync(Guid assignmentId, Guid userId);
    Task AddAsync(AssignmentCaptain captain);
    Task RemoveAsync(AssignmentCaptain captain);
    Task SaveChangesAsync();
}
