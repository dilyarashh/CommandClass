using FluentValidation;
using PotteryClass.Data.DTOs;
using PotteryClass.Data.Entities;
using PotteryClass.Data.Entities.Enums;
using PotteryClass.Data.Repositories;
using PotteryClass.Infrastructure.Auth;
using PotteryClass.Infrastructure.Errors.Exceptions;
using ValidationException = PotteryClass.Infrastructure.Errors.Exceptions.ValidationException;

namespace PotteryClass.Services;

public class CourseService(
    ICourseRepository repo,
    ICurrentUser currentUser,
    ICourseCodeGenerator codeGen,
    IValidator<CreateCourseRequest> validator,
    IValidator<JoinCourseRequest> joinValidator)
    : ICourseService
{
    public async Task<CourseDto> CreateCourseAsync(CreateCourseRequest dto)
    {
        if (currentUser.GetRole() != UserRole.Admin)
        {
            throw new ForbiddenException("Только администратор может создавать курсы");
        }

        var validationResult = await validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            throw new ValidationException(errors);
        }

        var userId = currentUser.GetUserId();

        string code;
        var attempts = 0;

        do
        {
            attempts++;
            if (attempts > 10)
            {
                throw new BadRequestException("Не удалось сгенерировать уникальный код курса");
            }

            code = codeGen.Generate();
        }
        while (await repo.CodeExistsAsync(code));

        var now = DateTime.UtcNow;
        var courseId = Guid.NewGuid();

        var course = new Course
        {
            Id = courseId,
            Name = dto.Name.Trim(),
            Description = dto.Description,
            Code = code,
            IsActive = true,
            CreatedByUserId = userId,
            CreatedAtUtc = now,
            Teachers = new List<CourseTeacher>
            {
                new()
                {
                    CourseId = courseId,
                    UserId = userId,
                    CreatedAtUtc = now
                }
            }
        };

        await repo.AddAsync(course);
        await repo.SaveChangesAsync();

        return new CourseDto
        {
            Id = course.Id,
            Name = course.Name,
            Description = course.Description,
            Code = course.Code,
            IsActive = course.IsActive
        };
    }

    public async Task<CourseDto> JoinCourseAsync(JoinCourseRequest dto)
    {
        var validationResult = await joinValidator.ValidateAsync(dto);

        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            throw new ValidationException(errors);
        }

        var userId = currentUser.GetUserId();

        var course = await repo.GetByCodeAsync(dto.Code.Trim());

        if (course == null)
        {
            throw new NotFoundException("Курс не найден");
        }

        if (!course.IsActive)
        {
            throw new BadRequestException("Курс архивирован");
        }

        var link = await repo.GetStudentLinkAsync(course.Id, userId);

        if (link != null)
        {
            if (link.IsBlocked)
            {
                throw new ForbiddenException("Вы заблокированы на курсе");
            }

            return new CourseDto
            {
                Id = course.Id,
                Name = course.Name,
                Description = course.Description,
                Code = course.Code,
                IsActive = course.IsActive
            };
        }

        var student = new CourseStudent
        {
            CourseId = course.Id,
            UserId = userId,
            CreatedAtUtc = DateTime.UtcNow,
            IsBlocked = false
        };

        await repo.AddStudentAsync(student);
        await repo.SaveChangesAsync();

        return new CourseDto
        {
            Id = course.Id,
            Name = course.Name,
            Description = course.Description,
            Code = course.Code,
            IsActive = course.IsActive
        };
    }

    public async Task<List<MyCourseDto>> GetMyCoursesAsync()
    {
        var userId = currentUser.GetUserId();

        var courses = await repo.GetUserCoursesAsync(userId);

        return courses.Select(c =>
        {
            var isTeacher = c.Teachers.Any(t => t.UserId == userId);

            return new MyCourseDto
            {
                Id = c.Id,
                Name = c.Name,
                Code = c.Code,
                Role = isTeacher ? "Teacher" : "Student"
            };
        }).ToList();
    }

    public async Task<CourseDto> GetCourseByIdAsync(Guid courseId)
    {
        var userId = currentUser.GetUserId();

        var course = await repo.GetByIdAsync(courseId);

        if (course == null)
        {
            throw new NotFoundException("Курс не найден");
        }

        var isTeacher = course.Teachers.Any(t => t.UserId == userId);

        var studentLink = course.Students
            .FirstOrDefault(s => s.UserId == userId);

        var isStudent = studentLink != null;

        if (!isTeacher && !isStudent)
        {
            throw new ForbiddenException("Вы не состоите в этом курсе");
        }

        if (studentLink?.IsBlocked == true)
        {
            throw new ForbiddenException("Вы заблокированы на курсе");
        }

        return new CourseDto
        {
            Id = course.Id,
            Name = course.Name,
            Description = course.Description,
            Code = course.Code,
            IsActive = course.IsActive
        };
    }

    public Task<List<CourseStudentDto>> GetCourseStudentsAsync(Guid courseId)
    {
        throw new NotImplementedException();
    }
}