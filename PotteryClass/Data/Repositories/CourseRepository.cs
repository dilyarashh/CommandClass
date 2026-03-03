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
}