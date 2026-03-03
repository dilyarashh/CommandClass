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
    IValidator<CreateCourseRequest> validator)
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
}