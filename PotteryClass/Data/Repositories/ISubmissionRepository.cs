using PotteryClass.Data.Entities;

namespace PotteryClass.Data.Repositories;

public interface ISubmissionRepository
{
    Task<Submission?> GetByIdAsync(Guid submissionId);
    Task SaveChangesAsync();
}