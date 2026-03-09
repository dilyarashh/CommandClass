using FluentAssertions;
using Moq;
using FluentValidation;
using PotteryClass.Data.DTOs;
using PotteryClass.Data.Entities;
using PotteryClass.Data.Entities.Enums;
using PotteryClass.Data.Repositories;
using PotteryClass.Infrastructure.Auth;
using PotteryClass.Infrastructure.Errors.Exceptions;
using PotteryClass.Services;
using Xunit;
using ValidationException = PotteryClass.Infrastructure.Errors.Exceptions.ValidationException;
using PotteryClass.Infrastructure.Validators;

namespace PotteryClassTests;

public class CourseServiceTests
{
    /// Тесты на создание курса

    [Fact]
    public async Task CreateCourse_Admin_CreatesCourse_AndAddsCreatorAsTeacher()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);
        var codeGen = new Mock<ICourseCodeGenerator>(MockBehavior.Strict);

        var adminId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId()).Returns(adminId);
        currentUser.Setup(x => x.GetRole()).Returns(UserRole.Admin);

        codeGen.Setup(x => x.Generate()).Returns("4xdqpxlk");
        repo.Setup(x => x.CodeExistsAsync("4xdqpxlk")).ReturnsAsync(false);

        Course? savedCourse = null;
        repo.Setup(x => x.AddAsync(It.IsAny<Course>()))
            .Callback<Course>(c => savedCourse = c)
            .Returns(Task.CompletedTask);

        repo.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        var createValidator = new CreateCourseValidator();
        var joinValidator = new JoinCourseValidator();

        var service = new CourseService(repo.Object, currentUser.Object, codeGen.Object, createValidator, joinValidator);

        var request = new CreateCourseRequest
        {
            Name = "Курс гончарки",
            Description = "Описание"
        };

        var result = await service.CreateCourseAsync(request);

        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Курс гончарки");
        result.Description.Should().Be("Описание");
        result.Code.Should().Be("4xdqpxlk");
        result.IsActive.Should().BeTrue();

        savedCourse.Should().NotBeNull();
        savedCourse!.Name.Should().Be("Курс гончарки");
        savedCourse.Code.Should().Be("4xdqpxlk");
        savedCourse.IsActive.Should().BeTrue();
        savedCourse.CreatedByUserId.Should().Be(adminId);

        savedCourse.Teachers.Should().ContainSingle(t => t.UserId == adminId);

        repo.VerifyAll();
        currentUser.VerifyAll();
        codeGen.VerifyAll();
    }

    [Fact]
    public async Task CreateCourse_NotAdmin_ThrowsForbidden()
    {
        var repo = new Mock<ICourseRepository>();
        var currentUser = new Mock<ICurrentUser>();
        var codeGen = new Mock<ICourseCodeGenerator>();

        var createValidator = new CreateCourseValidator();
        var joinValidator = new JoinCourseValidator();

        var service = new CourseService(repo.Object, currentUser.Object, codeGen.Object, createValidator, joinValidator);

        var act = () => service.CreateCourseAsync(new CreateCourseRequest { Name = "Курс" });

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task CreateCourse_WhenCodeCollides_Regenerates()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);
        var codeGen = new Mock<ICourseCodeGenerator>(MockBehavior.Strict);

        currentUser.Setup(x => x.GetUserId()).Returns(Guid.NewGuid());
        currentUser.Setup(x => x.GetRole()).Returns(UserRole.Admin);

        codeGen.SetupSequence(x => x.Generate())
            .Returns("duplica1")
            .Returns("unique12");

        repo.Setup(x => x.CodeExistsAsync("duplica1")).ReturnsAsync(true);
        repo.Setup(x => x.CodeExistsAsync("unique12")).ReturnsAsync(false);

        repo.Setup(x => x.AddAsync(It.IsAny<Course>())).Returns(Task.CompletedTask);
        repo.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        var createValidator = new CreateCourseValidator();
        var joinValidator = new JoinCourseValidator();

        var service = new CourseService(repo.Object, currentUser.Object, codeGen.Object, createValidator, joinValidator);

        var dto = await service.CreateCourseAsync(new CreateCourseRequest { Name = "Курс" });

        dto.Code.Should().Be("unique12");

        repo.VerifyAll();
        currentUser.VerifyAll();
        codeGen.VerifyAll();
    }

    [Fact]
    public async Task CreateCourse_EmptyName_ThrowsValidationException()
    {
        var repo = new Mock<ICourseRepository>();
        var currentUser = new Mock<ICurrentUser>();
        var codeGen = new Mock<ICourseCodeGenerator>();

        currentUser.Setup(x => x.GetRole()).Returns(UserRole.Admin);

        var createValidator = new CreateCourseValidator();
        var joinValidator = new JoinCourseValidator();

        var service = new CourseService(repo.Object, currentUser.Object, codeGen.Object, createValidator, joinValidator);

        var act = () => service.CreateCourseAsync(new CreateCourseRequest { Name = "   " });

        await act.Should().ThrowAsync<ValidationException>();
    }


    /// Тесты на получение присоединение к курсу по коду

    [Fact]
    public async Task JoinCourse_EmptyCode_ThrowsValidationException()
    {
        var repo = new Mock<ICourseRepository>();
        var currentUser = new Mock<ICurrentUser>();
        var codeGen = new Mock<ICourseCodeGenerator>();

        currentUser.Setup(x => x.GetUserId()).Returns(Guid.NewGuid());

        var validator = new CreateCourseValidator();
        var joinValidator = new JoinCourseValidator();

        var service = new CourseService(repo.Object, currentUser.Object, codeGen.Object, validator, joinValidator);

        var act = () => service.JoinCourseAsync(new JoinCourseRequest { Code = "   " });

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task JoinCourse_CourseNotFound_ThrowsNotFound()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);
        var codeGen = new Mock<ICourseCodeGenerator>();

        var userId = Guid.NewGuid();
        currentUser.Setup(x => x.GetUserId()).Returns(userId);

        repo.Setup(x => x.GetByCodeAsync("4xdqpxlk")).ReturnsAsync((Course?)null);

        var validator = new CreateCourseValidator();
        var joinValidator = new JoinCourseValidator();

        var service = new CourseService(repo.Object, currentUser.Object, codeGen.Object, validator, joinValidator);

        var act = () => service.JoinCourseAsync(new JoinCourseRequest { Code = "4xdqpxlk" });

        await act.Should().ThrowAsync<NotFoundException>();

        repo.VerifyAll();
        currentUser.VerifyAll();
    }

    [Fact]
    public async Task JoinCourse_CourseIsArchived_ThrowsBadRequest()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);
        var codeGen = new Mock<ICourseCodeGenerator>();

        var userId = Guid.NewGuid();
        currentUser.Setup(x => x.GetUserId()).Returns(userId);

        var course = new Course
        {
            Id = Guid.NewGuid(),
            Name = "Курс",
            Code = "4xdqpxlk",
            IsActive = false
        };

        repo.Setup(x => x.GetByCodeAsync("4xdqpxlk")).ReturnsAsync(course);

        var validator = new CreateCourseValidator();
        var joinValidator = new JoinCourseValidator();

        var service = new CourseService(repo.Object, currentUser.Object, codeGen.Object, validator, joinValidator);

        var act = () => service.JoinCourseAsync(new JoinCourseRequest { Code = "4xdqpxlk" });

        await act.Should().ThrowAsync<BadRequestException>();

        repo.VerifyAll();
        currentUser.VerifyAll();
    }

    [Fact]
    public async Task JoinCourse_WhenAlreadyStudent_ReturnsCourse_AndDoesNotAddLink()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);
        var codeGen = new Mock<ICourseCodeGenerator>();

        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId()).Returns(userId);

        var course = new Course
        {
            Id = courseId,
            Name = "Курс",
            Code = "4xdqpxlk",
            IsActive = true
        };

        repo.Setup(x => x.GetByCodeAsync("4xdqpxlk")).ReturnsAsync(course);
        repo.Setup(x => x.GetStudentLinkAsync(courseId, userId))
            .ReturnsAsync(new CourseStudent { CourseId = courseId, UserId = userId, IsBlocked = false });

        var validator = new CreateCourseValidator();
        var joinValidator = new JoinCourseValidator();

        var service = new CourseService(repo.Object, currentUser.Object, codeGen.Object, validator, joinValidator);

        var dto = await service.JoinCourseAsync(new JoinCourseRequest { Code = "4xdqpxlk" });

        dto.Id.Should().Be(courseId);
        dto.Code.Should().Be("4xdqpxlk");

        repo.Verify(x => x.AddStudentAsync(It.IsAny<CourseStudent>()), Times.Never);
        repo.Verify(x => x.SaveChangesAsync(), Times.Never);
        repo.VerifyAll();
        currentUser.VerifyAll();
    }

    [Fact]
    public async Task JoinCourse_WhenBlocked_ThrowsForbidden()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);
        var codeGen = new Mock<ICourseCodeGenerator>();

        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId()).Returns(userId);

        var course = new Course
        {
            Id = courseId,
            Name = "Курс",
            Code = "4xdqpxlk",
            IsActive = true
        };

        repo.Setup(x => x.GetByCodeAsync("4xdqpxlk")).ReturnsAsync(course);
        repo.Setup(x => x.GetStudentLinkAsync(courseId, userId))
            .ReturnsAsync(new CourseStudent { CourseId = courseId, UserId = userId, IsBlocked = true });

        var validator = new CreateCourseValidator();
        var joinValidator = new JoinCourseValidator();

        var service = new CourseService(repo.Object, currentUser.Object, codeGen.Object, validator, joinValidator);

        var act = () => service.JoinCourseAsync(new JoinCourseRequest { Code = "4xdqpxlk" });

        await act.Should().ThrowAsync<ForbiddenException>();

        repo.VerifyAll();
        currentUser.VerifyAll();
    }

    [Fact]
    public async Task JoinCourse_HappyPath_AddsStudentLink()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);
        var codeGen = new Mock<ICourseCodeGenerator>();

        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId()).Returns(userId);

        var course = new Course
        {
            Id = courseId,
            Name = "Курс",
            Code = "4xdqpxlk",
            IsActive = true
        };

        repo.Setup(x => x.GetByCodeAsync("4xdqpxlk")).ReturnsAsync(course);
        repo.Setup(x => x.GetStudentLinkAsync(courseId, userId)).ReturnsAsync((CourseStudent?)null);

        CourseStudent? saved = null;
        repo.Setup(x => x.AddStudentAsync(It.IsAny<CourseStudent>()))
            .Callback<CourseStudent>(x => saved = x)
            .Returns(Task.CompletedTask);

        repo.Setup(x => x.SaveChangesAsync()).Returns(Task.CompletedTask);

        var validator = new CreateCourseValidator();
        var joinValidator = new JoinCourseValidator();

        var service = new CourseService(repo.Object, currentUser.Object, codeGen.Object, validator, joinValidator);

        var dto = await service.JoinCourseAsync(new JoinCourseRequest { Code = "4xdqpxlk" });

        dto.Id.Should().Be(courseId);

        saved.Should().NotBeNull();
        saved!.CourseId.Should().Be(courseId);
        saved.UserId.Should().Be(userId);
        saved.IsBlocked.Should().BeFalse();

        repo.VerifyAll();
        currentUser.VerifyAll();
    }


    /// Тесты на получение списка своих курсов

    [Fact]
    public async Task GetMyCourses_ReturnsCoursesFromRepository()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var userId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId()).Returns(userId);

        var courses = new List<Course>
    {
        new()
        {
            Id = Guid.NewGuid(),
            Name = "Гончарка",
            Code = "abcd1234",
            Teachers = new List<CourseTeacher>(),
            Students = new List<CourseStudent>()
        }
    };

        repo.Setup(x => x.GetUserCoursesAsync(userId))
            .ReturnsAsync(courses);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        var result = await service.GetMyCoursesAsync();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("Гончарка");

        repo.VerifyAll();
        currentUser.VerifyAll();
    }

    [Fact]
    public async Task GetMyCourses_WhenUserHasNoCourses_ReturnsEmptyList()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var userId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId()).Returns(userId);

        repo.Setup(x => x.GetUserCoursesAsync(userId))
            .ReturnsAsync(new List<Course>());

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        var result = await service.GetMyCoursesAsync();

        result.Should().BeEmpty();

        repo.VerifyAll();
        currentUser.VerifyAll();
    }


    /// Тесты на получение информации о курсе по айди

    [Fact]
    public async Task GetCourseById_WhenCourseNotExists_ThrowsNotFound()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var courseId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId()).Returns(userId);

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync((Course?)null);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        var act = () => service.GetCourseByIdAsync(courseId);

        await act.Should().ThrowAsync<NotFoundException>();

        repo.VerifyAll();
        currentUser.VerifyAll();
    }

    [Fact]
    public async Task GetCourseById_WhenUserNotParticipant_ThrowsForbidden()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var courseId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId()).Returns(userId);

        var course = new Course
        {
            Id = courseId,
            Name = "Курс",
            Code = "abcd1234",
            Teachers = new List<CourseTeacher>(),
            Students = new List<CourseStudent>()
        };

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        var act = () => service.GetCourseByIdAsync(courseId);

        await act.Should().ThrowAsync<ForbiddenException>();

        repo.VerifyAll();
    }

    [Fact]
    public async Task GetCourseById_WhenUserIsStudent_ReturnsCourse()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var courseId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId()).Returns(userId);

        var course = new Course
        {
            Id = courseId,
            Name = "Курс",
            Code = "abcd1234",
            Teachers = new List<CourseTeacher>(),
            Students = new List<CourseStudent>
        {
            new()
            {
                UserId = userId,
                CourseId = courseId,
                IsBlocked = false
            }
        }
        };

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        var result = await service.GetCourseByIdAsync(courseId);

        result.Id.Should().Be(courseId);
        result.Name.Should().Be("Курс");

        repo.VerifyAll();
    }

    [Fact]
    public async Task GetCourseById_WhenUserIsTeacher_ReturnsCourse()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var courseId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId()).Returns(userId);

        var course = new Course
        {
            Id = courseId,
            Name = "Курс",
            Code = "abcd1234",
            Teachers = new List<CourseTeacher>
        {
            new()
            {
                UserId = userId,
                CourseId = courseId
            }
        },
            Students = new List<CourseStudent>()
        };

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        var result = await service.GetCourseByIdAsync(courseId);

        result.Id.Should().Be(courseId);

        repo.VerifyAll();
    }


    /// Тесты на получение списка учеников курса

    [Fact]
    public async Task GetCourseStudents_WhenCourseNotExists_ThrowsNotFound()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var courseId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId()).Returns(userId);

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync((Course?)null);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        var act = () => service.GetCourseStudentsAsync(courseId);

        await act.Should().ThrowAsync<NotFoundException>();

        repo.VerifyAll();
    }

    [Fact]
    public async Task GetCourseStudents_WhenUserIsTeacher_ReturnsStudents()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var courseId = Guid.NewGuid();
        var teacherId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId()).Returns(teacherId);

        var course = new Course
        {
            Id = courseId,
            Teachers = new List<CourseTeacher>
        {
            new() { UserId = teacherId }
        }
        };

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        repo.Setup(x => x.GetCourseStudentsAsync(courseId))
            .ReturnsAsync(new List<User>
            {
            new()
            {
                Id = Guid.NewGuid(),
                FirstName = "Ivan",
                LastName = "Ivanov",
                Email = "ivan@test.com"
            }
            });

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        var result = await service.GetCourseStudentsAsync(courseId);

        result.Should().HaveCount(1);

        repo.VerifyAll();
    }

    [Fact]
    public async Task GetCourseStudents_WhenUserIsStudent_ThrowsForbidden()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var courseId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId()).Returns(userId);

        var course = new Course
        {
            Id = courseId,
            Teachers = new List<CourseTeacher>(),
            Students = new List<CourseStudent>
        {
            new()
            {
                CourseId = courseId,
                UserId = userId
            }
        }
        };

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        var act = () => service.GetCourseStudentsAsync(courseId);

        await act.Should().ThrowAsync<ForbiddenException>();

        repo.VerifyAll();
    }


    /// Тесты на блокировку ученика

    [Fact]
    public async Task BlockStudent_WhenCourseNotExists_ThrowsNotFound()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var teacherId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var studentId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId()).Returns(teacherId);

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync((Course?)null);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        Func<Task> act = () => service.BlockStudentAsync(courseId, studentId);

        await act.Should().ThrowAsync<NotFoundException>();

        repo.VerifyAll();
    }

    [Fact]
    public async Task BlockStudent_WhenUserNotTeacher_ThrowsForbidden()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var studentId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId()).Returns(userId);

        var course = new Course
        {
            Id = courseId,
            Teachers = new List<CourseTeacher>()
        };

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        Func<Task> act = () => service.BlockStudentAsync(courseId, studentId);

        await act.Should().ThrowAsync<ForbiddenException>();

        repo.VerifyAll();
    }

    [Fact]
    public async Task BlockStudent_WhenStudentNotExists_ThrowsNotFound()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var teacherId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var studentId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId()).Returns(teacherId);

        var course = new Course
        {
            Id = courseId,
            Teachers = new List<CourseTeacher>
        {
            new() { UserId = teacherId }
        },
            Students = new List<CourseStudent>()
        };

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        Func<Task> act = () => service.BlockStudentAsync(courseId, studentId);

        await act.Should().ThrowAsync<NotFoundException>();

        repo.VerifyAll();
    }

    [Fact]
    public async Task BlockStudent_WhenStudentAlreadyBlocked_ThrowsBadRequest()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var teacherId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var studentId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId()).Returns(teacherId);

        var course = new Course
        {
            Id = courseId,
            Teachers = new List<CourseTeacher>
        {
            new() { UserId = teacherId }
        },
            Students = new List<CourseStudent>
        {
            new()
            {
                UserId = studentId,
                IsBlocked = true
            }
        }
        };

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        Func<Task> act = () => service.BlockStudentAsync(courseId, studentId);

        await act.Should().ThrowAsync<BadRequestException>();

        repo.VerifyAll();
    }

    [Fact]
    public async Task BlockStudent_WhenValid_BlocksStudent()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var teacherId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var studentId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId()).Returns(teacherId);

        var student = new CourseStudent
        {
            UserId = studentId,
            IsBlocked = false
        };

        var course = new Course
        {
            Id = courseId,
            Teachers = new List<CourseTeacher>
        {
            new() { UserId = teacherId }
        },
            Students = new List<CourseStudent>
        {
            student
        }
        };

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        repo.Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        await service.BlockStudentAsync(courseId, studentId);

        student.IsBlocked.Should().BeTrue();

        repo.VerifyAll();
    }


    /// Тесты на разблокировку ранее заблокированного ученика

    [Fact]
    public async Task UnblockStudent_WhenCourseNotExists_ThrowsNotFound()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var teacherId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var studentId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId()).Returns(teacherId);

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync((Course?)null);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        Func<Task> act = () => service.UnblockStudentAsync(courseId, studentId);

        await act.Should().ThrowAsync<NotFoundException>();

        repo.VerifyAll();
    }

    [Fact]
    public async Task UnblockStudent_WhenUserNotTeacher_ThrowsForbidden()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var studentId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId()).Returns(userId);

        var course = new Course
        {
            Id = courseId,
            Teachers = new List<CourseTeacher>()
        };

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        Func<Task> act = () => service.UnblockStudentAsync(courseId, studentId);

        await act.Should().ThrowAsync<ForbiddenException>();

        repo.VerifyAll();
    }

    [Fact]
    public async Task UnblockStudent_WhenStudentNotExists_ThrowsNotFound()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var teacherId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var studentId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId()).Returns(teacherId);

        var course = new Course
        {
            Id = courseId,
            Teachers = new List<CourseTeacher>
        {
            new() { UserId = teacherId }
        },
            Students = new List<CourseStudent>()
        };

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        Func<Task> act = () => service.UnblockStudentAsync(courseId, studentId);

        await act.Should().ThrowAsync<NotFoundException>();

        repo.VerifyAll();
    }

    [Fact]
    public async Task UnblockStudent_WhenStudentNotBlocked_ThrowsBadRequest()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var teacherId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var studentId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId()).Returns(teacherId);

        var course = new Course
        {
            Id = courseId,
            Teachers = new List<CourseTeacher>
        {
            new() { UserId = teacherId }
        },
            Students = new List<CourseStudent>
        {
            new()
            {
                UserId = studentId,
                IsBlocked = false
            }
        }
        };

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        Func<Task> act = () => service.UnblockStudentAsync(courseId, studentId);

        await act.Should().ThrowAsync<BadRequestException>();

        repo.VerifyAll();
    }

    [Fact]
    public async Task UnblockStudent_WhenValid_UnblocksStudent()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var teacherId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var studentId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId()).Returns(teacherId);

        var student = new CourseStudent
        {
            UserId = studentId,
            IsBlocked = true
        };

        var course = new Course
        {
            Id = courseId,
            Teachers = new List<CourseTeacher>
        {
            new() { UserId = teacherId }
        },
            Students = new List<CourseStudent>
        {
            student
        }
        };

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        repo.Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        await service.UnblockStudentAsync(courseId, studentId);

        student.IsBlocked.Should().BeFalse();

        repo.VerifyAll();
    }


    /// Тесты на назначение пользователя преподавателем

    [Fact]
    public async Task AddTeacher_WhenCourseNotExists_ThrowsNotFound()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var adminId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var teacherId = Guid.NewGuid();

        currentUser.Setup(x => x.GetRole()).Returns(UserRole.Admin);

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync((Course?)null);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        Func<Task> act = () => service.AddTeacherAsync(courseId, teacherId);

        await act.Should().ThrowAsync<NotFoundException>();

        repo.VerifyAll();
    }

    [Fact]
    public async Task AddTeacher_WhenUserNotAdmin_ThrowsForbidden()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var courseId = Guid.NewGuid();
        var teacherId = Guid.NewGuid();

        currentUser.Setup(x => x.GetRole()).Returns(UserRole.Teacher);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        Func<Task> act = () => service.AddTeacherAsync(courseId, teacherId);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task AddTeacher_WhenAlreadyTeacher_ThrowsBadRequest()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var adminId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var teacherId = Guid.NewGuid();

        currentUser.Setup(x => x.GetRole()).Returns(UserRole.Admin);

        var course = new Course
        {
            Id = courseId,
            Teachers = new List<CourseTeacher>
        {
            new() { UserId = teacherId }
        }
        };

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        Func<Task> act = () => service.AddTeacherAsync(courseId, teacherId);

        await act.Should().ThrowAsync<BadRequestException>();

        repo.VerifyAll();
    }

    [Fact]
    public async Task AddTeacher_WhenValid_AddsTeacher()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var adminId = Guid.NewGuid();
        var courseId = Guid.NewGuid();
        var teacherId = Guid.NewGuid();

        currentUser.Setup(x => x.GetRole()).Returns(UserRole.Admin);

        var course = new Course
        {
            Id = courseId,
            Teachers = new List<CourseTeacher>()
        };

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        repo.Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        await service.AddTeacherAsync(courseId, teacherId);

        course.Teachers.Should().Contain(x => x.UserId == teacherId);

        repo.VerifyAll();
    }


    /// Тесты на удаление пользователя с роли учителя

    [Fact]
    public async Task RemoveTeacher_WhenUserNotAdmin_ThrowsForbidden()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        currentUser.Setup(x => x.GetRole()).Returns(UserRole.Teacher);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        Func<Task> act = () => service.RemoveTeacherAsync(Guid.NewGuid(), Guid.NewGuid());

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task RemoveTeacher_WhenCourseNotExists_ThrowsNotFound()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var courseId = Guid.NewGuid();

        currentUser.Setup(x => x.GetRole()).Returns(UserRole.Admin);

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync((Course?)null);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        Func<Task> act = () => service.RemoveTeacherAsync(courseId, Guid.NewGuid());

        await act.Should().ThrowAsync<NotFoundException>();

        repo.VerifyAll();
    }

    [Fact]
    public async Task RemoveTeacher_WhenTeacherNotInCourse_ThrowsNotFound()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var courseId = Guid.NewGuid();
        var teacherId = Guid.NewGuid();

        currentUser.Setup(x => x.GetRole()).Returns(UserRole.Admin);

        var course = new Course
        {
            Id = courseId,
            Teachers = new List<CourseTeacher>()
        };

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        Func<Task> act = () => service.RemoveTeacherAsync(courseId, teacherId);

        await act.Should().ThrowAsync<NotFoundException>();

        repo.VerifyAll();
    }

    [Fact]
    public async Task RemoveTeacher_WhenLastTeacher_ThrowsBadRequest()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var courseId = Guid.NewGuid();
        var teacherId = Guid.NewGuid();

        currentUser.Setup(x => x.GetRole()).Returns(UserRole.Admin);

        var course = new Course
        {
            Id = courseId,
            Teachers = new List<CourseTeacher>
        {
            new() { UserId = teacherId }
        }
        };

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        Func<Task> act = () => service.RemoveTeacherAsync(courseId, teacherId);

        await act.Should().ThrowAsync<BadRequestException>();

        repo.VerifyAll();
    }

    [Fact]
    public async Task RemoveTeacher_WhenValid_RemovesTeacher()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var courseId = Guid.NewGuid();
        var teacherId = Guid.NewGuid();

        currentUser.Setup(x => x.GetRole()).Returns(UserRole.Admin);

        var course = new Course
        {
            Id = courseId,
            Teachers = new List<CourseTeacher>
        {
            new() { UserId = teacherId },
            new() { UserId = Guid.NewGuid() }
        }
        };

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        repo.Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        await service.RemoveTeacherAsync(courseId, teacherId);

        course.Teachers.Should().NotContain(t => t.UserId == teacherId);

        repo.VerifyAll();
    }


    /// Тесты на архивирование курса

    [Fact]
    public async Task ArchiveCourse_WhenUserNotAdmin_ThrowsForbidden()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var courseId = Guid.NewGuid();

        currentUser.Setup(x => x.GetRole()).Returns(UserRole.Student);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        Func<Task> act = () => service.ArchiveCourseAsync(courseId);

        await act.Should()
            .ThrowAsync<ForbiddenException>()
            .WithMessage("Только администратор может архивировать курсы");
    }

    [Fact]
    public async Task ArchiveCourse_WhenCourseNotExists_ThrowsNotFound()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var courseId = Guid.NewGuid();

        currentUser.Setup(x => x.GetRole()).Returns(UserRole.Admin);

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync((Course?)null);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        Func<Task> act = () => service.ArchiveCourseAsync(courseId);

        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Курс не найден");

        repo.VerifyAll();
    }

    [Fact]
    public async Task ArchiveCourse_WhenAlreadyArchived_ThrowsBadRequest()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var courseId = Guid.NewGuid();

        var course = new Course
        {
            Id = courseId,
            IsActive = false
        };

        currentUser.Setup(x => x.GetRole()).Returns(UserRole.Admin);

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        Func<Task> act = () => service.ArchiveCourseAsync(courseId);

        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Курс уже архивирован");

        repo.VerifyAll();
    }

    [Fact]
    public async Task ArchiveCourse_WhenValid_ArchivesCourse()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var courseId = Guid.NewGuid();

        var course = new Course
        {
            Id = courseId,
            IsActive = true
        };

        currentUser.Setup(x => x.GetRole()).Returns(UserRole.Admin);

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        repo.Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        await service.ArchiveCourseAsync(courseId);

        course.IsActive.Should().BeFalse();

        repo.VerifyAll();
    }


    /// Тесты на разархивирование курса

    [Fact]
    public async Task RestoreCourse_WhenUserNotAdmin_ThrowsForbidden()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var courseId = Guid.NewGuid();

        currentUser.Setup(x => x.GetRole()).Returns(UserRole.Student);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        Func<Task> act = () => service.RestoreCourseAsync(courseId);

        await act.Should()
            .ThrowAsync<ForbiddenException>()
            .WithMessage("Только администратор может разархивировать курсы");
    }

    [Fact]
    public async Task RestoreCourse_WhenCourseNotExists_ThrowsNotFound()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var courseId = Guid.NewGuid();

        currentUser.Setup(x => x.GetRole()).Returns(UserRole.Admin);

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync((Course?)null);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        Func<Task> act = () => service.RestoreCourseAsync(courseId);

        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Курс не найден");

        repo.VerifyAll();
    }

    [Fact]
    public async Task RestoreCourse_WhenCourseAlreadyActive_ThrowsBadRequest()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var courseId = Guid.NewGuid();

        var course = new Course
        {
            Id = courseId,
            IsActive = true
        };

        currentUser.Setup(x => x.GetRole()).Returns(UserRole.Admin);

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        Func<Task> act = () => service.RestoreCourseAsync(courseId);

        await act.Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("Курс уже активен");

        repo.VerifyAll();
    }

    [Fact]
    public async Task RestoreCourse_WhenValid_RestoresCourse()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var courseId = Guid.NewGuid();

        var course = new Course
        {
            Id = courseId,
            IsActive = false
        };

        currentUser.Setup(x => x.GetRole()).Returns(UserRole.Admin);

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        repo.Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        await service.RestoreCourseAsync(courseId);

        course.IsActive.Should().BeTrue();

        repo.VerifyAll();
    }


    /// Тесты на получение списка всех курсов админом

    [Fact]
    public async Task GetAllCourses_WhenUserNotAdmin_ThrowsForbidden()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        currentUser.Setup(x => x.GetRole()).Returns(UserRole.Student);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        Func<Task> act = () => service.GetAllCoursesAsync();

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task GetAllCourses_WhenAdmin_ReturnsCourses()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        currentUser.Setup(x => x.GetRole()).Returns(UserRole.Admin);

        var courses = new List<Course>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Pottery",
                Code = "abc123",
                Description = "Basic pottery",
                IsActive = true
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Advanced pottery",
                Code = "def456",
                Description = null,
                IsActive = false
            }
        };

        repo.Setup(x => x.GetAllAsync())
            .ReturnsAsync(courses);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        var result = await service.GetAllCoursesAsync();

        result.Should().HaveCount(2);

        result[0].Name.Should().Be("Pottery");
        result[1].IsActive.Should().BeFalse();

        repo.VerifyAll();
    }


    /// Тесты на редактирование курса администратором

    [Fact]
    public async Task UpdateCourse_WhenUserNotAdmin_ThrowsForbidden()
    {
        var repo = new Mock<ICourseRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var courseId = Guid.NewGuid();

        currentUser.Setup(x => x.GetRole()).Returns(UserRole.Student);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        Func<Task> act = () => service.UpdateCourseAsync(courseId, new UpdateCourseRequest
        {
            Name = "New name"
        });

        await act.Should()
            .ThrowAsync<ForbiddenException>()
            .WithMessage("Только администратор может редактировать курсы");
    }

    [Fact]
    public async Task UpdateCourse_WhenCourseNotExists_ThrowsNotFound()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var courseId = Guid.NewGuid();

        currentUser.Setup(x => x.GetRole()).Returns(UserRole.Admin);

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync((Course?)null);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        Func<Task> act = () => service.UpdateCourseAsync(courseId, new UpdateCourseRequest
        {
            Name = "New name"
        });

        await act.Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("Курс не найден");

        repo.VerifyAll();
    }

    [Fact]
    public async Task UpdateCourse_WhenValid_UpdatesCourse()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var courseId = Guid.NewGuid();

        var course = new Course
        {
            Id = courseId,
            Name = "Old name",
            Description = "Old desc"
        };

        currentUser.Setup(x => x.GetRole()).Returns(UserRole.Admin);

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        repo.Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        await service.UpdateCourseAsync(courseId, new UpdateCourseRequest
        {
            Name = "New name",
            Description = "New desc"
        });

        course.Name.Should().Be("New name");
        course.Description.Should().Be("New desc");

        repo.VerifyAll();
    }


    /// Тесты на покидание курса

    [Fact]
    public async Task LeaveCourse_WhenCourseNotExists_ThrowsNotFound()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var courseId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId()).Returns(userId);

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync((Course?)null);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        Func<Task> act = () => service.LeaveCourseAsync(courseId);

        await act.Should().ThrowAsync<NotFoundException>();

        repo.VerifyAll();
    }

    [Fact]
    public async Task LeaveCourse_WhenUserNotParticipant_ThrowsBadRequest()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var courseId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId()).Returns(userId);

        var course = new Course
        {
            Id = courseId,
            Teachers = new List<CourseTeacher>(),
            Students = new List<CourseStudent>()
        };

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        Func<Task> act = () => service.LeaveCourseAsync(courseId);

        await act.Should().ThrowAsync<BadRequestException>();

        repo.VerifyAll();
    }

    [Fact]
    public async Task LeaveCourse_WhenStudent_RemovesStudentLink()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var courseId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId()).Returns(userId);

        var student = new CourseStudent
        {
            UserId = userId
        };

        var course = new Course
        {
            Id = courseId,
            Students = new List<CourseStudent> { student },
            Teachers = new List<CourseTeacher>()
        };

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        repo.Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        await service.LeaveCourseAsync(courseId);

        course.Students.Should().NotContain(student);

        repo.VerifyAll();
    }

    [Fact]
    public async Task LeaveCourse_WhenCreator_ThrowsBadRequest()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var courseId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId()).Returns(userId);

        var course = new Course
        {
            Id = courseId,
            CreatedByUserId = userId,
            Teachers = new List<CourseTeacher>
        {
            new() { UserId = userId },
            new() { UserId = Guid.NewGuid() }
        }
        };

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        Func<Task> act = () => service.LeaveCourseAsync(courseId);

        await act.Should().ThrowAsync<BadRequestException>();

        repo.VerifyAll();
    }

    [Fact]
    public async Task LeaveCourse_WhenTeacherAndNotLast_RemovesTeacher()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var courseId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId()).Returns(userId);

        var teacher = new CourseTeacher
        {
            UserId = userId
        };

        var course = new Course
        {
            Id = courseId,
            Teachers = new List<CourseTeacher>
        {
            teacher,
            new() { UserId = Guid.NewGuid() }
        },
            Students = new List<CourseStudent>()
        };

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        repo.Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        await service.LeaveCourseAsync(courseId);

        course.Teachers.Should().NotContain(t => t.UserId == userId);

        repo.VerifyAll();
    }

    [Fact]
    public async Task LeaveCourse_WhenLastTeacher_ThrowsBadRequest()
    {
        var repo = new Mock<ICourseRepository>(MockBehavior.Strict);
        var currentUser = new Mock<ICurrentUser>(MockBehavior.Strict);

        var courseId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId()).Returns(userId);

        var course = new Course
        {
            Id = courseId,
            Teachers = new List<CourseTeacher>
        {
            new() { UserId = userId }
        },
            Students = new List<CourseStudent>()
        };

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(course);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            Mock.Of<ICourseCodeGenerator>(),
            Mock.Of<IValidator<CreateCourseRequest>>(),
            Mock.Of<IValidator<JoinCourseRequest>>()
        );

        Func<Task> act = () => service.LeaveCourseAsync(courseId);

        await act.Should().ThrowAsync<BadRequestException>();

        repo.VerifyAll();
    }


    /// Тесты на получение списка преподавателей курса

    [Fact]
    public async Task GetCourseTeachers_WhenCourseNotExists_ThrowsNotFound()
    {
        var repo = new Mock<ICourseRepository>();
        var currentUser = new Mock<ICurrentUser>();
        var codeGen = new Mock<ICourseCodeGenerator>();
        var validator = new Mock<IValidator<CreateCourseRequest>>();
        var joinValidator = new Mock<IValidator<JoinCourseRequest>>();

        var courseId = Guid.NewGuid();

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync((Course?)null);

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            codeGen.Object,
            validator.Object,
            joinValidator.Object);

        Func<Task> act = () => service.GetCourseTeachersAsync(courseId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetCourseTeachers_WhenUserNotParticipant_ThrowsForbidden()
    {
        var repo = new Mock<ICourseRepository>();
        var currentUser = new Mock<ICurrentUser>();
        var codeGen = new Mock<ICourseCodeGenerator>();
        var validator = new Mock<IValidator<CreateCourseRequest>>();
        var joinValidator = new Mock<IValidator<JoinCourseRequest>>();

        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId())
            .Returns(userId);

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(new Course
            {
                Id = courseId,
                Teachers = new List<CourseTeacher>(),
                Students = new List<CourseStudent>()
            });

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            codeGen.Object,
            validator.Object,
            joinValidator.Object);

        Func<Task> act = () => service.GetCourseTeachersAsync(courseId);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task GetCourseTeachers_WhenValid_ReturnsTeachers()
    {
        var repo = new Mock<ICourseRepository>();
        var currentUser = new Mock<ICurrentUser>();
        var codeGen = new Mock<ICourseCodeGenerator>();
        var validator = new Mock<IValidator<CreateCourseRequest>>();
        var joinValidator = new Mock<IValidator<JoinCourseRequest>>();

        var userId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId())
            .Returns(userId);

        repo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(new Course
            {
                Id = courseId,
                Teachers = new List<CourseTeacher>
                {
                new CourseTeacher { UserId = userId }
                },
                Students = new List<CourseStudent>()
            });

        repo.Setup(x => x.GetCourseTeachersAsync(courseId))
            .ReturnsAsync(new List<User>
            {
            new User
            {
                Id = userId,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@test.com"
            }
            });

        var service = new CourseService(
            repo.Object,
            currentUser.Object,
            codeGen.Object,
            validator.Object,
            joinValidator.Object);

        var result = await service.GetCourseTeachersAsync(courseId);

        result.Should().HaveCount(1);
        result.First().Email.Should().Be("john@test.com");
    }
}