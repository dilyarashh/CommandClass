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
    
    public async Task AddAsync(Submission submission)
    {
        db.Submissions.Add(submission);
        await db.SaveChangesAsync();
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
    
    public async Task<List<Submission>> GetByAssignmentAsync(Guid assignmentId)
    {
        return await db.Submissions
            .Include(x => x.Files)
            .Where(x => x.AssignmentId == assignmentId)
            .OrderByDescending(x => x.Created)
            .ToListAsync();
    }
}