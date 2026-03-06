using PotteryClass.Data.DTOs;

namespace PotteryClass.Services;

public interface ICourseService
{
    Task<CourseDto> CreateCourseAsync(CreateCourseRequest dto);

    Task<CourseDto> JoinCourseAsync(JoinCourseRequest dto);

    Task<List<MyCourseDto>> GetMyCoursesAsync();

    Task<CourseDto> GetCourseByIdAsync(Guid courseId);

    Task<List<CourseStudentDto>> GetCourseStudentsAsync(Guid courseId);

    Task BlockStudentAsync(Guid courseId, Guid studentId);

    Task UnblockStudentAsync(Guid courseId, Guid studentId);

    Task AddTeacherAsync(Guid courseId, Guid teacherId);

    Task RemoveTeacherAsync(Guid courseId, Guid teacherId);
}