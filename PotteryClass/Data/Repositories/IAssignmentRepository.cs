using PotteryClass.Data.Entities;

namespace PotteryClass.Data.Repositories;

public interface IAssignmentRepository
{
    Task AddAsync(Assignment assignment);

    Task<Assignment?> GetByIdAsync(Guid id);

    Task UpdateAsync(Assignment assignment);

    Task DeleteAsync(Assignment assignment);
    Task AddFileAsync(AssignmentFile file);
}