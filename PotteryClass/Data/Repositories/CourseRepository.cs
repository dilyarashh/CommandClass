using Microsoft.EntityFrameworkCore;
using PotteryClass.Data.Entities;

namespace PotteryClass.Data.Repositories;

public class CourseRepository(AppDbContext db) : ICourseRepository
{
    public Task<bool> CodeExistsAsync(string code)
        => db.Courses.AnyAsync(c => c.Code == code);

    public async Task AddAsync(Course course)
        => await db.Courses.AddAsync(course);

    public Task SaveChangesAsync()
        => db.SaveChangesAsync();

    public Task<Course?> GetByCodeAsync(string code)
        => db.Courses.FirstOrDefaultAsync(x => x.Code == code);

    public Task<CourseStudent?> GetStudentLinkAsync(Guid courseId, Guid userId)
        => db.CourseStudents
            .FirstOrDefaultAsync(x => x.CourseId == courseId && x.UserId == userId);

    public async Task AddStudentAsync(CourseStudent link)
        => await db.CourseStudents.AddAsync(link);
}