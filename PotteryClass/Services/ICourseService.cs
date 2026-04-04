using PotteryClass.Data.DTOs;
using PotteryClass.Data.Entities.Enums;

namespace PotteryClass.Services;

public interface ICourseService
{
    Task<CourseDto> CreateCourseAsync(CreateCourseRequest dto);

    Task<CourseDto> JoinCourseAsync(JoinCourseRequest dto);

    Task<List<MyCourseDto>> GetMyCoursesAsync(MyCoursesFilter filter);

    Task<CourseDto> GetCourseByIdAsync(Guid courseId);

    Task<List<CourseStudentDto>> GetCourseStudentsAsync(Guid courseId);

    Task AddStudentAsync(Guid courseId, Guid studentId);

    Task BlockStudentAsync(Guid courseId, Guid studentId);

    Task UnblockStudentAsync(Guid courseId, Guid studentId);

    Task AddTeacherAsync(Guid courseId, Guid teacherId);

    Task RemoveTeacherAsync(Guid courseId, Guid teacherId);

    Task ArchiveCourseAsync(Guid courseId);

    Task RestoreCourseAsync(Guid courseId);

    Task<List<CourseDto>> GetAllCoursesAsync();

    Task UpdateCourseAsync(Guid courseId, UpdateCourseRequest dto);

    Task LeaveCourseAsync(Guid courseId);

    Task<List<CourseTeacherDto>> GetCourseTeachersAsync(Guid courseId);
}
