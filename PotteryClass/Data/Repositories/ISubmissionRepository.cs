using PotteryClass.Data.Entities;

namespace PotteryClass.Data.Repositories;

public interface ISubmissionRepository
{
    Task<Submission?> GetByIdAsync(Guid submissionId);
    Task SaveChangesAsync();
    Task AddAsync(Submission submission);
    Task<Submission?> GetByAssignmentAndStudentAsync(Guid assignmentId, Guid studentId);
    Task UpdateAsync(Submission submission);
}