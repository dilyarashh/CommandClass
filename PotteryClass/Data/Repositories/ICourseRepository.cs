using PotteryClass.Data.Entities;

namespace PotteryClass.Data.Repositories;

public interface ICourseRepository
{
    Task<bool> CodeExistsAsync(string code);
    Task AddAsync(Course course);
    Task SaveChangesAsync();

    Task<Course?> GetByCodeAsync(string code);
    Task<CourseStudent?> GetStudentLinkAsync(Guid courseId, Guid userId);
    Task AddStudentAsync(CourseStudent link);
}