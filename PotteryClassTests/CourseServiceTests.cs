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

        var validator = new CreateCourseValidator();

        var service = new CourseService(repo.Object, currentUser.Object, codeGen.Object, validator);

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

        var validator = new CreateCourseValidator();

        var service = new CourseService(repo.Object, currentUser.Object, codeGen.Object, validator);

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

        var validator = new CreateCourseValidator();
        var service = new CourseService(repo.Object, currentUser.Object, codeGen.Object, validator);

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

        var validator = new CreateCourseValidator();

        var service = new CourseService(repo.Object, currentUser.Object, codeGen.Object, validator);

        var act = () => service.CreateCourseAsync(new CreateCourseRequest { Name = "   " });

        await act.Should().ThrowAsync<ValidationException>();
    }
}