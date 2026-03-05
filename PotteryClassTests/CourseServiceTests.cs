using FluentAssertions;
using Moq;
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

        currentUser.Setup(x => x.GetRole()).Returns(UserRole.Student);

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




    [Fact]
    public async Task JoinCourse_EmptyCode_ThrowsValidationException()
    {
        var repo = new Mock<ICourseRepository>();
        var currentUser = new Mock<ICurrentUser>();
        var codeGen = new Mock<ICourseCodeGenerator>();

        currentUser.Setup(x => x.GetUserId()).Returns(Guid.NewGuid());
        currentUser.Setup(x => x.GetRole()).Returns(UserRole.Student);

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
        currentUser.Setup(x => x.GetRole()).Returns(UserRole.Student);

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
        currentUser.Setup(x => x.GetRole()).Returns(UserRole.Student);

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
        currentUser.Setup(x => x.GetRole()).Returns(UserRole.Student);

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
        currentUser.Setup(x => x.GetRole()).Returns(UserRole.Student);

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
        currentUser.Setup(x => x.GetRole()).Returns(UserRole.Student);

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
}