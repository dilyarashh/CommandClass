using PotteryClass.Data.Entities;

namespace PotteryClass.Data.Repositories;

public interface ISubmissionRepository
{
    public Task AddAsync(Submission submission);
    public Task<Submission?> GetByIdAsync(Guid id);
    Task<Submission?> GetByAssignmentAndStudentAsync(Guid assignmentId, Guid studentId);
    public Task UpdateAsync(Submission submission);
}