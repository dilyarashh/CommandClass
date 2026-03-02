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
    
    [Fact]
    public async Task GetByIdAsync_Should_Return_UserDto()
    {
        var id = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(new User
            {
                Id = id,
                Email = "test@mail.ru",
                Role = UserRole.Student
            });

        var service = CreateService();

        var result = await service.GetByIdAsync(id);

        result.Id.Should().Be(id);
    }
    
    [Fact]
    public async Task GetAllAsync_Should_Return_Empty_When_No_Users()
    {
        var query = new UsersQuery();

        _repoMock.Setup(r => r.GetAllAsync(query))
            .ReturnsAsync(new PagedResult<User>
            {
                TotalCount = 0,
                Items = new List<User>()
            });

        var service = CreateService();

        var result = await service.GetAllAsync(query);

        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }
    
    [Fact]
    public async Task GetAllAsync_Should_Return_All_Users()
    {
        var query = new UsersQuery();

        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), FirstName = "A", Email = "a@mail.com", Role = UserRole.Student },
            new() { Id = Guid.NewGuid(), FirstName = "B", Email = "b@mail.com", Role = UserRole.Teacher }
        };

        _repoMock.Setup(r => r.GetAllAsync(query))
            .ReturnsAsync(new PagedResult<User>
            {
                TotalCount = users.Count,
                Items = users
            });

        var service = CreateService();

        var result = await service.GetAllAsync(query);

        result.TotalCount.Should().Be(2);
        result.Items.Select(u => u.FirstName).Should().Contain(new[] { "A", "B" });
    }
    
    [Fact]
    public async Task GetAllAsync_Should_Filter_By_Role()
    {
        var query = new UsersQuery { Role = UserRole.Student };

        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), Email = "student@mail.com", Role = UserRole.Student },
            new() { Id = Guid.NewGuid(), Email = "teacher@mail.com", Role = UserRole.Teacher }
        };

        _repoMock.Setup(r => r.GetAllAsync(query))
            .ReturnsAsync(new PagedResult<User>
            {
                TotalCount = 1,
                Items = users.Where(u => u.Role == UserRole.Student).ToList()
            });

        var service = CreateService();

        var result = await service.GetAllAsync(query);

        result.TotalCount.Should().Be(1);
        result.Items.All(u => u.Role == UserRole.Student).Should().BeTrue();
    }
    
    [Fact]
    public async Task GetAllAsync_Should_Filter_By_Search()
    {
        var query = new UsersQuery { Search = "ivan" };

        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), FirstName = "Ivan", Email = "ivan@mail.com", Role = UserRole.Student },
            new() { Id = Guid.NewGuid(), FirstName = "Petr", Email = "petr@mail.com", Role = UserRole.Student }
        };

        _repoMock.Setup(r => r.GetAllAsync(query))
            .ReturnsAsync(new PagedResult<User>
            {
                TotalCount = 1,
                Items = users.Where(u => u.FirstName.Contains("ivan", StringComparison.OrdinalIgnoreCase)
                                         || u.Email.Contains("ivan", StringComparison.OrdinalIgnoreCase))
                    .ToList()
            });

        var service = CreateService();

        var result = await service.GetAllAsync(query);

        result.TotalCount.Should().Be(1);
        result.Items.All(u => u.FirstName.Contains("Ivan") || u.Email.Contains("ivan")).Should().BeTrue();
    }
    
    [Fact]
    public async Task GetAllAsync_Should_Sort_By_FirstName()
    {
        var query = new UsersQuery { SortBy = "FirstName", Desc = false };

        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), FirstName = "C", Email = "c@mail.com", Role = UserRole.Student },
            new() { Id = Guid.NewGuid(), FirstName = "A", Email = "a@mail.com", Role = UserRole.Student },
            new() { Id = Guid.NewGuid(), FirstName = "B", Email = "b@mail.com", Role = UserRole.Student }
        };

        _repoMock.Setup(r => r.GetAllAsync(query))
            .ReturnsAsync(new PagedResult<User>
            {
                TotalCount = 3,
                Items = users.OrderBy(u => u.FirstName).ToList()
            });

        var service = CreateService();

        var result = await service.GetAllAsync(query);

        result.Items.Select(u => u.FirstName).Should().ContainInOrder("A", "B", "C");
    }
    
    [Fact]
    public async Task GetAllAsync_Should_Respect_Pagination()
    {
        var query = new UsersQuery { Page = 2, PageSize = 2 };

        var allUsers = new List<User>
        {
            new() { Id = Guid.NewGuid(), FirstName = "A", Email = "a@mail.com", Role = UserRole.Student },
            new() { Id = Guid.NewGuid(), FirstName = "B", Email = "b@mail.com", Role = UserRole.Student },
            new() { Id = Guid.NewGuid(), FirstName = "C", Email = "c@mail.com", Role = UserRole.Student }
        };

        _repoMock.Setup(r => r.GetAllAsync(query))
            .ReturnsAsync(new PagedResult<User>
            {
                TotalCount = allUsers.Count,
                Items = allUsers.Skip(2).Take(2).ToList() // страница 2
            });

        var service = CreateService();

        var result = await service.GetAllAsync(query);

        result.TotalCount.Should().Be(3);
        result.Items.Should().HaveCount(1);
        result.Items.First().FirstName.Should().Be("C");
    }
}