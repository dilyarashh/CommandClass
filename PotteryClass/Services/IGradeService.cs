using PotteryClass.Data.DTOs;

namespace PotteryClass.Services;

public interface IGradeService
{
    Task<SubmissionGradeDto> SetGradeAsync(Guid submissionId, SetSubmissionGradeRequest dto);
    Task<SubmissionAssessmentFormDto> GetAssessmentFormAsync(Guid submissionId);
    Task<SubmissionAssessmentDto> GetAssessmentAsync(Guid submissionId);
    Task<SubmissionAssessmentDto> SaveAssessmentAsync(Guid submissionId, SaveSubmissionAssessmentRequest dto);
    Task DeleteGradeAsync(Guid submissionId);
    Task<List<CourseStudentGradeDto>> GetCourseGradesAsync(Guid courseId);
    Task<List<MyCourseGradeDto>> GetMyCourseGradesAsync(Guid courseId);
    Task<List<AssignmentTeamGradeDto>> GetAssignmentTeamGradesAsync(Guid assignmentId);
    Task<AssignmentTeamGradeDto> GetMyTeamGradeAsync(Guid assignmentId);
}
