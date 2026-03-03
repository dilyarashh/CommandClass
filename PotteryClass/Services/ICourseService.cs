using PotteryClass.Data.DTOs;

namespace PotteryClass.Services;

public interface ICourseService
{
    Task<CourseDto> CreateCourseAsync(CreateCourseRequest dto);
}