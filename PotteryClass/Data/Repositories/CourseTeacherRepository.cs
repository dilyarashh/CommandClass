using Microsoft.EntityFrameworkCore;

namespace PotteryClass.Data.Repositories;

public class CourseTeacherRepository(AppDbContext db) : ICourseTeacherRepository
{
    public async Task<bool> IsTeacherAsync(Guid courseId, Guid userId)
    {
        return await db.CourseTeachers
            .AnyAsync(x => x.CourseId == courseId && x.UserId == userId);
    }
}