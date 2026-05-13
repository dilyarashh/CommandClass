using Microsoft.EntityFrameworkCore;
using PotteryClass.Data.Entities;

namespace PotteryClass.Data.Repositories;

public class SubmissionAssessmentRepository(AppDbContext db) : ISubmissionAssessmentRepository
{
    public Task<SubmissionAssessment?> GetBySubmissionIdAsync(Guid submissionId)
        => db.SubmissionAssessments.FirstOrDefaultAsync(x => x.SubmissionId == submissionId);

    public async Task AddAsync(SubmissionAssessment assessment)
        => await db.SubmissionAssessments.AddAsync(assessment);

    public Task SaveChangesAsync()
        => db.SaveChangesAsync();
}
