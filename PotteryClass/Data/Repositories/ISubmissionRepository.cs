using PotteryClass.Data.Entities;
using PotteryClass.Data.DTOs;

namespace PotteryClass.Data.Repositories;

public interface ISubmissionRepository
{
    Task<Submission?> GetByIdAsync(Guid submissionId);
    Task SaveChangesAsync();
    Task AddAsync(Submission submission);
    Task<Submission?> GetByAssignmentAndStudentAsync(Guid assignmentId, Guid studentId);
    Task UpdateAsync(Submission submission);
    Task<List<Submission>> GetByAssignmentAsync(Guid assignmentId);
    Task<List<CourseStudentGradeDto>> GetCourseGradesAsync(Guid courseId);
}