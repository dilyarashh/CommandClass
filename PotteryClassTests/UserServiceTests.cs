using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using PotteryClass.Data.DTOs;
using PotteryClass.Data.Entities;
using PotteryClass.Data.Entities.Enums;
using PotteryClass.Data.Repositories;
using PotteryClass.Infrastructure.Auth;
using PotteryClass.Services;
using Xunit;
using Assert = Xunit.Assert;

namespace PotteryClassTests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _repoMock = new();
    private readonly Mock<IValidator<RegistrationRequest>> _validatorMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();

    private UserService CreateService()
        => new(_repoMock.Object, _validatorMock.Object, _currentUserMock.Object);
    
    [Fact]
    public async Task CreateUserAsync_Should_Create_User_When_Valid()
    {
        var dto = new RegistrationRequest
        {
            FirstName = "Имя",
            LastName = "Фамилия",
            MiddleName = "Отчество",
            Email = "email@mail.ru",
            Password = "12345678"
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult());

        var service = CreateService();

        var result = await service.CreateUserAsync(dto);

        result.Should().NotBeNull();
        result.Email.Should().Be(dto.Email);

        _repoMock.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
    }

    [Fact]
    public async Task CreateUserAsync_Should_Throw_When_Invalid()
    {
        var dto = new RegistrationRequest
        {
            FirstName = "Имя",
            LastName = "Фамилия",
            MiddleName = "Отчество",
            Email = "email@mail.ru",
            Password = "12345678"
        };

        var failures = new List<ValidationFailure>
        {
            new("Email", "Email required")
        };

        _validatorMock
            .Setup(v => v.ValidateAsync(dto, default))
            .ReturnsAsync(new ValidationResult(failures));

        var service = CreateService();

        await Assert.ThrowsAsync<
            PotteryClass.Infrastructure.Errors.Exceptions.ValidationException>(
            () => service.CreateUserAsync(dto));
    }
    
    [Fact]
    public async Task GetCurrentUserAsync_Should_Return_UserDto()
    {
        var userId = Guid.NewGuid();

        _currentUserMock.Setup(x => x.GetUserId()).Returns(userId);

        _repoMock.Setup(r => r.GetByIdAsync(userId))
            .ReturnsAsync(new User
            {
                Id = userId,
                FirstName = "Имя",
                LastName = "Фамилия",
                MiddleName = "Отчество",
                Email = "email@mail.ru",
                Role = UserRole.Student
            });

        var service = CreateService();

        var result = await service.GetCurrentUserAsync();

        result.Id.Should().Be(userId);
        result.Email.Should().Be("email@mail.ru");
    }

    [Fact]
    public async Task UpdateProfileAsync_Should_Update_Only_NotNull_Fields()
    {
        var userId = Guid.NewGuid();

        var user = new User
        {
            FirstName = "Имя",
            LastName = "Фамилия",
            MiddleName = "Отчество",
            Email = "email@mail.ru",
            Role = UserRole.Student
        };

        _currentUserMock.Setup(x => x.GetUserId()).Returns(userId);
        _repoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var dto = new UpdateProfileRequest
        {
            FirstName = "Новое имя",
            LastName = null
        };

        var service = CreateService();

        var result = await service.UpdateProfileAsync(dto);

        result.FirstName.Should().Be("Новое имя");
        result.LastName.Should().Be("Фамилия"); 

        _repoMock.Verify(r => r.UpdateAsync(user), Times.Once);
    }

    [Fact]
    public async Task DeleteCurrentUserAsync_Should_Call_Delete()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId };

        _currentUserMock.Setup(x => x.GetUserId()).Returns(userId);
        _repoMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

        var service = CreateService();

        await service.DeleteCurrentUserAsync();

        _repoMock.Verify(r => r.DeleteAsync(user), Times.Once);
    }
}