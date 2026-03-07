using PotteryClass.Data.Entities;
using PotteryClass.Data.DTOs;

namespace PotteryClass.Data.Repositories;

public interface ICourseRepository
{
    Task<bool> CodeExistsAsync(string code);
    Task AddAsync(Course course);
    Task SaveChangesAsync();

    Task<Course?> GetByCodeAsync(string code);
    Task<CourseStudent?> GetStudentLinkAsync(Guid courseId, Guid userId);
    Task AddStudentAsync(CourseStudent link);

    Task<List<Course>> GetUserCoursesAsync(Guid userId);

    Task<Course?> GetByIdAsync(Guid courseId);

    Task<List<User>> GetCourseStudentsAsync(Guid courseId);

    Task<List<Course>> GetAllAsync();
}