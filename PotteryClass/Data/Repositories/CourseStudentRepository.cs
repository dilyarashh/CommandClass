using Microsoft.EntityFrameworkCore;
using PotteryClass.Data.Entities;

namespace PotteryClass.Data.Repositories;

public class CourseStudentRepository(AppDbContext db) : ICourseStudentRepository
{
    public async Task<bool> IsStudentAsync(Guid courseId, Guid userId)
    {
        return await db.CourseStudents
            .AnyAsync(x =>
                x.CourseId == courseId &&
                x.UserId == userId &&
                !x.IsBlocked);
    }

    public async Task<List<Guid>> GetActiveStudentIdsAsync(Guid courseId)
    {
        return await db.CourseStudents
            .Where(x => x.CourseId == courseId && !x.IsBlocked)
            .Select(x => x.UserId)
            .ToListAsync();
    }

    public async Task<List<User>> GetActiveStudentsAsync(Guid courseId)
    {
        return await db.CourseStudents
            .Where(x => x.CourseId == courseId && !x.IsBlocked)
            .Select(x => x.User)
            .ToListAsync();
    }
}
