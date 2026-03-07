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

    public async Task<List<CourseStudentDto>> GetCourseStudentsAsync(Guid courseId)
    {
        var userId = currentUser.GetUserId();

        var course = await repo.GetByIdAsync(courseId);

        if (course == null)
        {
            throw new NotFoundException("Курс не найден");
        }

        var isTeacher = course.Teachers.Any(t => t.UserId == userId);

        if (!isTeacher)
        {
            throw new ForbiddenException("Только преподаватель может смотреть список студентов");
        }

        var students = await repo.GetCourseStudentsAsync(courseId);

        return students.Select(u => new CourseStudentDto
        {
            Id = u.Id,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email
        }).ToList();
    }

    public async Task BlockStudentAsync(Guid courseId, Guid studentId)
    {
        var teacherId = currentUser.GetUserId();

        var course = await repo.GetByIdAsync(courseId);

        if (course == null)
            throw new NotFoundException("Курс не найден");

        var isTeacher = course.Teachers.Any(t => t.UserId == teacherId);

        if (!isTeacher)
            throw new ForbiddenException("Только преподаватель курса может блокировать студентов");

        var student = course.Students.FirstOrDefault(s => s.UserId == studentId);

        if (student == null)
            throw new NotFoundException("Студент не найден");

        if (student.IsBlocked)
            throw new BadRequestException("Студент уже заблокирован");

        student.IsBlocked = true;

        await repo.SaveChangesAsync();
    }

    public async Task UnblockStudentAsync(Guid courseId, Guid studentId)
    {
        var teacherId = currentUser.GetUserId();

        var course = await repo.GetByIdAsync(courseId);

        if (course == null)
            throw new NotFoundException("Курс не найден");

        var isTeacher = course.Teachers.Any(t => t.UserId == teacherId);

        if (!isTeacher)
            throw new ForbiddenException("Только преподаватель курса может разблокировать студентов");

        var student = course.Students.FirstOrDefault(s => s.UserId == studentId);

        if (student == null)
            throw new NotFoundException("Студент не найден");

        if (!student.IsBlocked)
            throw new BadRequestException("Студент не заблокирован");

        student.IsBlocked = false;

        await repo.SaveChangesAsync();
    }

    public async Task AddTeacherAsync(Guid courseId, Guid teacherId)
    {
        if (currentUser.GetRole() != UserRole.Admin)
        {
            throw new ForbiddenException("Только администратор может назначать преподавателей");
        }    

        var course = await repo.GetByIdAsync(courseId);

        if (course == null)
        {
            throw new NotFoundException("Курс не найден");
        }

        var alreadyTeacher = course.Teachers.Any(t => t.UserId == teacherId);

        if (alreadyTeacher)
        {
            throw new BadRequestException("Пользователь уже является преподавателем курса");
        }

        course.Teachers.Add(new CourseTeacher
        {
            CourseId = courseId,
            UserId = teacherId,
            CreatedAtUtc = DateTime.UtcNow
        });

        await repo.SaveChangesAsync();
    }

    public async Task RemoveTeacherAsync(Guid courseId, Guid teacherId)
    {
        if (currentUser.GetRole() != UserRole.Admin)
        {
            throw new ForbiddenException("Только администратор может удалять преподавателей");
        }

        var course = await repo.GetByIdAsync(courseId);

        if (course == null)
        {
            throw new NotFoundException("Курс не найден");
        }

        var teacher = course.Teachers.FirstOrDefault(t => t.UserId == teacherId);

        if (teacher == null)
        {
            throw new NotFoundException("Преподаватель не найден на курсе");
        }

        if (course.CreatedByUserId == teacherId)
        {
            throw new BadRequestException("Нельзя удалить создателя курса");
        }

        if (course.Teachers.Count == 1)
        {
            throw new BadRequestException("Нельзя удалить последнего преподавателя курса");
        }

        course.Teachers.Remove(teacher);

        await repo.SaveChangesAsync();
    }

    public async Task ArchiveCourseAsync(Guid courseId)
    {
        if (currentUser.GetRole() != UserRole.Admin)
        {
            throw new ForbiddenException("Только администратор может архивировать курсы");
        }

        var course = await repo.GetByIdAsync(courseId);

        if (course == null)
        {
            throw new NotFoundException("Курс не найден");
        }

        if (!course.IsActive)
        {
            throw new BadRequestException("Курс уже архивирован");
        }

        course.IsActive = false;

        await repo.SaveChangesAsync();
    }

    public async Task RestoreCourseAsync(Guid courseId)
    {
        if (currentUser.GetRole() != UserRole.Admin)
        {
            throw new ForbiddenException("Только администратор может разархивировать курсы");
        }

        var course = await repo.GetByIdAsync(courseId);

        if (course == null)
        {
            throw new NotFoundException("Курс не найден");
        }

        if (course.IsActive)
        {
            throw new BadRequestException("Курс уже активен");
        }

        course.IsActive = true;

        await repo.SaveChangesAsync();
    }

    public async Task<List<CourseDto>> GetAllCoursesAsync()
    {
        if (currentUser.GetRole() != UserRole.Admin)
            throw new ForbiddenException("Только администратор может смотреть все курсы");

        var courses = await repo.GetAllAsync();

        return courses.Select(c => new CourseDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            Code = c.Code,
            IsActive = c.IsActive
        }).ToList();
    }

    public async Task UpdateCourseAsync(Guid courseId, UpdateCourseRequest dto)
    {
        throw new NotImplementedException();
    }
}