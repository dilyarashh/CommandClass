using Microsoft.EntityFrameworkCore;
using PotteryClass.Data.Entities;

namespace PotteryClass.Data.Repositories;

public class SubmissionRepository(AppDbContext db) : ISubmissionRepository
{
    public async Task AddAsync(Submission submission)
    {
        db.Submissions.Add(submission);
        await db.SaveChangesAsync();
    }

    public async Task<Submission?> GetByIdAsync(Guid id)
    {
        return await db.Submissions
            .Include(x => x.Files)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<Submission?> GetByAssignmentAndStudentAsync(Guid assignmentId, Guid studentId)
    {
        return await db.Submissions
            .Include(x => x.Files)
            .FirstOrDefaultAsync(x =>
                x.AssignmentId == assignmentId &&
                x.StudentId == studentId);
    }

    public async Task UpdateAsync(Submission submission)
    {
        db.Submissions.Update(submission);
        await db.SaveChangesAsync();
    }
}