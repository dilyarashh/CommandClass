using PotteryClass.Data.DTOs;

namespace PotteryClass.Services;

public interface ICourseService
{
    Task<CourseDto> CreateCourseAsync(CreateCourseRequest dto);
    Task<CourseDto> JoinCourseAsync(JoinCourseRequest dto);
    Task<List<MyCourseDto>> GetMyCoursesAsync();
    Task<CourseDto> GetCourseByIdAsync(Guid courseId);
}