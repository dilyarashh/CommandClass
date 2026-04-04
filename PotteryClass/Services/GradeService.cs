using PotteryClass.Data.DTOs;
using PotteryClass.Data.Entities.Enums;
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
        var role = currentUser.GetRole();

        var isTeacher = course.Teachers.Any(t => t.UserId == teacherId);

        if (role != UserRole.Admin && !isTeacher)
        {
            throw new ForbiddenException("Только преподаватель курса или администратор может ставить оценки");
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
        var role = currentUser.GetRole();

        var isTeacher = course.Teachers.Any(t => t.UserId == teacherId);

        if (role != UserRole.Admin && !isTeacher)
        {
            throw new ForbiddenException("Только преподаватель курса или администратор может удалять оценки");
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
        var role = currentUser.GetRole();

        var isTeacher = course.Teachers.Any(t => t.UserId == teacherId);

        if (role != UserRole.Admin && !isTeacher)
        {
            throw new ForbiddenException("Только преподаватель курса или администратор может смотреть успеваемость");
        }

        return await submissionRepo.GetCourseGradesAsync(courseId);
    }

    public async Task<List<MyCourseGradeDto>> GetMyCourseGradesAsync(Guid courseId)
    {
        var course = await courseRepo.GetByIdAsync(courseId);

        if (course == null)
        {
            throw new NotFoundException("Курс не найден");
        }

        var studentId = currentUser.GetUserId();
        var role = currentUser.GetRole();

        if (role == UserRole.Admin)
        {
            return await submissionRepo.GetStudentCourseGradesAsync(courseId, studentId);
        }

        var student = course.Students.FirstOrDefault(s => s.UserId == studentId);

        if (student == null)
        {
            throw new ForbiddenException("Только студент курса может смотреть свои оценки");
        }

        if (student.IsBlocked)
        {
            throw new ForbiddenException("Вы заблокированы на курсе");
        }

        return await submissionRepo.GetStudentCourseGradesAsync(courseId, studentId);
    }
}
