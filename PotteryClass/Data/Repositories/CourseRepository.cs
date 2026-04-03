using Microsoft.EntityFrameworkCore;
using PotteryClass.Data.Entities;
using PotteryClass.Data.DTOs;

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
        => db.Courses
            .Include(x => x.Teachers)
            .Include(x => x.Students)
            .FirstOrDefaultAsync(x => x.Code == code);

    public Task<CourseStudent?> GetStudentLinkAsync(Guid courseId, Guid userId)
        => db.CourseStudents
            .FirstOrDefaultAsync(x => x.CourseId == courseId && x.UserId == userId);

    public async Task AddStudentAsync(CourseStudent link)
        => await db.CourseStudents.AddAsync(link);

    public async Task<List<Course>> GetUserCoursesAsync(Guid userId)
    {
        return await db.Courses
            .Include(c => c.Teachers)
            .Include(c => c.Students)
            .Where(c =>
                c.Teachers.Any(t => t.UserId == userId) ||
                c.Students.Any(s => s.UserId == userId && !s.IsBlocked))
            .AsNoTracking()
            .ToListAsync();
    }

    public Task<Course?> GetByIdAsync(Guid courseId)
    => db.Courses
        .Include(x => x.Teachers)
        .Include(x => x.Students)
        .FirstOrDefaultAsync(x => x.Id == courseId);

    public Task<List<CourseStudent>> GetCourseStudentsAsync(Guid courseId)
    => db.CourseStudents
            .Include(x => x.User)
            .Where(x => x.CourseId == courseId)
            .ToListAsync();

    public Task<List<Course>> GetAllAsync()
    => db.Courses
            .Include(x => x.Teachers)
            .Include(x => x.Students)
            .AsNoTracking()
            .ToListAsync();

    public async Task<List<User>> GetCourseTeachersAsync(Guid courseId)
    {
        return await db.CourseTeachers
            .Where(x => x.CourseId == courseId)
            .Join(
                db.Users,
                ct => ct.UserId,
                u => u.Id,
                (ct, u) => u)
            .ToListAsync();
    }
}
