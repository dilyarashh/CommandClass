using PotteryClass.Data.Entities;

namespace PotteryClass.Data.Repositories;

public interface ISubmissionAssessmentRepository
{
    Task<SubmissionAssessment?> GetBySubmissionIdAsync(Guid submissionId);
    Task AddAsync(SubmissionAssessment assessment);
    void Delete(SubmissionAssessment assessment);
    Task SaveChangesAsync();
}
