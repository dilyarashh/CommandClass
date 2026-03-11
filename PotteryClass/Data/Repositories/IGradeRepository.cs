using PotteryClass.Data.Entities;

namespace PotteryClass.Data.Repositories;

public interface IGradeRepository
{
    Task AddAsync(Grade grade);

    Task<bool> ExistsAsync(Guid assignmentId, Guid studentId);

    Task SaveChangesAsync();
}