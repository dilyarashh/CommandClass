using PotteryClass.Data.DTOs;

namespace PotteryClass.Services;

public interface IGradeService
{
    Task<SubmissionGradeDto> SetGradeAsync(Guid submissionId, SetSubmissionGradeRequest dto);
    Task DeleteGradeAsync(Guid submissionId);
    Task<List<CourseStudentGradeDto>> GetCourseGradesAsync(Guid courseId);
    Task<List<MyCourseGradeDto>> GetMyCourseGradesAsync(Guid courseId);
}