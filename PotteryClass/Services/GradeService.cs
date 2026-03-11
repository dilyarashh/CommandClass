using PotteryClass.Data.DTOs;
using PotteryClass.Data.Entities;
using PotteryClass.Data.Repositories;
using PotteryClass.Infrastructure.Auth;
using PotteryClass.Infrastructure.Errors.Exceptions;

namespace PotteryClass.Services;

public class GradeService(
    IAssignmentRepository assignmentRepo,
    IGradeRepository gradeRepo,
    ICourseRepository courseRepo,
    ICurrentUser currentUser)
    : IGradeService
{
    public async Task<GradeDto> CreateGradeAsync(Guid assignmentId, CreateGradeRequest dto)
    {
        var assignment = await assignmentRepo.GetByIdAsync(assignmentId);

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

        var student = course.Students.FirstOrDefault(s => s.UserId == dto.StudentId);

        if (student == null)
        {
            throw new NotFoundException("Студент не найден на курсе");
        }

        if (student.IsBlocked)
        {
            throw new ForbiddenException("Студент заблокирован на курсе");
        }

        var gradeExists = await gradeRepo.ExistsAsync(assignmentId, dto.StudentId);

        if (gradeExists)
        {
            throw new BadRequestException("Оценка уже поставлена");
        }

        var grade = new Grade
        {
            Id = Guid.NewGuid(),
            AssignmentId = assignmentId,
            StudentId = dto.StudentId,
            TeacherId = teacherId,
            Value = dto.Value,
            CreatedAtUtc = DateTime.UtcNow
        };

        await gradeRepo.AddAsync(grade);
        await gradeRepo.SaveChangesAsync();

        return new GradeDto
        {
            Id = grade.Id,
            AssignmentId = grade.AssignmentId,
            StudentId = grade.StudentId,
            TeacherId = grade.TeacherId,
            Value = grade.Value
        };
    }

    public async Task DeleteGradeAsync(Guid gradeId)
    {
        var grade = await gradeRepo.GetByIdAsync(gradeId);

        if (grade == null)
        {
            throw new NotFoundException("Оценка не найдена");
        }

        var assignment = await assignmentRepo.GetByIdAsync(grade.AssignmentId);

        if (assignment == null)
        {
            throw new NotFoundException("Задание не найдено");
        }

        var course = await courseRepo.GetByIdAsync(assignment.CourseId);

        if (course == null)
        {
            throw new NotFoundException("Курс не найден");
        }

        var userId = currentUser.GetUserId();

        var isTeacher = course.Teachers.Any(t => t.UserId == userId);

        if (!isTeacher)
        {
            throw new ForbiddenException("Только преподаватель курса может удалять оценки");
        }

        gradeRepo.Delete(grade);

        await gradeRepo.SaveChangesAsync();
    }
}