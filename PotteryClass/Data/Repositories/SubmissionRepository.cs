using Microsoft.EntityFrameworkCore;
using PotteryClass.Data.Entities;

namespace PotteryClass.Data.Repositories;

public class SubmissionRepository(AppDbContext db) : ISubmissionRepository
{
    public async Task<Submission?> GetByIdAsync(Guid submissionId)
    {
        return await db.Submissions.FirstOrDefaultAsync(x => x.Id == submissionId);
    }

    public async Task SaveChangesAsync()
    {
        await db.SaveChangesAsync();
    }
}