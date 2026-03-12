using PotteryClass.Data.DTOs;
using PotteryClass.Data.Repositories;
using PotteryClass.Infrastructure.Auth;
using PotteryClass.Infrastructure.Errors.Exceptions;

namespace PotteryClass.Services;

public class GradeService(
    ISubmissionRepository submissionRepo,
    IAssignmentRepository assignmentRepo,
    ICourseRepository courseRepo,
    ICurrentUser currentUser)
    : IGradeService
{
    public async Task<SubmissionGradeDto> SetGradeAsync(Guid submissionId, SetSubmissionGradeRequest dto)
    {
        var submission = await submissionRepo.GetByIdAsync(submissionId);

        if (submission == null)
        {
            throw new NotFoundException("Решение не найдено");
        }

        var assignment = await assignmentRepo.GetByIdAsync(submission.AssignmentId);

        if (assignment == null)
        {
            throw new NotFoundException("Задание не найдено");
        }

        var course = await courseRepo.GetByIdAsync(assignment.CourseId);

        if (course == null)
        {
            throw new NotFoundException("Курс не найден");
        }

        var teacherId = currentUser.GetUserId();

        var isTeacher = course.Teachers.Any(t => t.UserId == teacherId);

        if (!isTeacher)
        {
            throw new ForbiddenException("Только преподаватель курса может ставить оценки");
        }

        submission.Grade = dto.Value;
        submission.GradedByTeacherId = teacherId;
        submission.GradedAtUtc = DateTime.UtcNow;

        await submissionRepo.SaveChangesAsync();

        return new SubmissionGradeDto
        {
            SubmissionId = submission.Id,
            AssignmentId = submission.AssignmentId,
            StudentId = submission.StudentId,
            Grade = submission.Grade,
            GradedByTeacherId = submission.GradedByTeacherId,
            GradedAtUtc = submission.GradedAtUtc
        };
    }

    public async Task DeleteGradeAsync(Guid submissionId)
    {
        var submission = await submissionRepo.GetByIdAsync(submissionId);

        if (submission == null)
        {
            throw new NotFoundException("Решение не найдено");
        }

        var assignment = await assignmentRepo.GetByIdAsync(submission.AssignmentId);

        if (assignment == null)
        {
            throw new NotFoundException("Задание не найдено");
        }

        var course = await courseRepo.GetByIdAsync(assignment.CourseId);

        if (course == null)
        {
            throw new NotFoundException("Курс не найден");
        }

        var teacherId = currentUser.GetUserId();

        var isTeacher = course.Teachers.Any(t => t.UserId == teacherId);

        if (!isTeacher)
        {
            throw new ForbiddenException("Только преподаватель курса может удалять оценки");
        }

        submission.Grade = null;
        submission.GradedByTeacherId = null;
        submission.GradedAtUtc = null;

        await submissionRepo.SaveChangesAsync();
    }

    public async Task<List<CourseStudentGradeDto>> GetCourseGradesAsync(Guid courseId)
    {
        var course = await courseRepo.GetByIdAsync(courseId);

        if (course == null)
        {
            throw new NotFoundException("Курс не найден");
        }

        var teacherId = currentUser.GetUserId();

        var isTeacher = course.Teachers.Any(t => t.UserId == teacherId);

        if (!isTeacher)
        {
            throw new ForbiddenException("Только преподаватель курса может смотреть успеваемость");
        }

        return await submissionRepo.GetCourseGradesAsync(courseId);
    }
}