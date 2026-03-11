using Microsoft.EntityFrameworkCore;
using PotteryClass.Data.Entities;

namespace PotteryClass.Data.Repositories;

public class GradeRepository(AppDbContext db) : IGradeRepository
{
    public async Task AddAsync(Grade grade)
    {
        await db.Grades.AddAsync(grade);
    }

    public async Task<bool> ExistsAsync(Guid assignmentId, Guid studentId)
    {
        return await db.Grades.AnyAsync(x =>
            x.AssignmentId == assignmentId &&
            x.StudentId == studentId);
    }

    public async Task SaveChangesAsync()
    {
        await db.SaveChangesAsync();
    }

    public async Task<Grade?> GetByIdAsync(Guid gradeId)
    {
        return await db.Grades.FindAsync(gradeId);
    }

    public void Delete(Grade grade)
    {
        db.Grades.Remove(grade);
    }
}