using FluentAssertions;
using Moq;
using PotteryClass.Data.DTOs;
using PotteryClass.Data.Entities;
using PotteryClass.Data.Entities.Enums;
using PotteryClass.Data.Repositories;
using PotteryClass.Infrastructure.Auth;
using PotteryClass.Services;
using Xunit;

namespace PotteryClassTests;

public class AssignmentServiceTests
{
    private readonly Mock<IAssignmentRepository> _repoMock = new();
    private readonly Mock<ICurrentUser> _currentUserMock = new();
    private readonly Mock<ICourseStudentRepository> _courseStudentMock = new();
    private readonly Mock<ICourseTeacherRepository> _courseTeacherMock = new();
    private readonly Mock<IFileStorageService> _storageService = new();

    private AssignmentService CreateService()
        => new(_repoMock.Object, _courseTeacherMock.Object,  _courseStudentMock.Object, _currentUserMock.Object, _storageService.Object);

    [Fact]
    public async Task CreateAsync_Should_Create_Assignment()
    {
        var dto = new CreateAssignmentRequest
        {
            CourseId = Guid.NewGuid(),
            Title = "Test assignment",
            Text = "Text",
            RequiresSubmission = true,
            Deadline = DateTime.UtcNow.AddDays(7)
        };

        _currentUserMock.Setup(x => x.GetRole())
            .Returns(UserRole.Admin);

        var service = CreateService();

        var result = await service.CreateAsync(dto);

        result.Title.Should().Be(dto.Title);

        _repoMock.Verify(r => r.AddAsync(It.IsAny<Assignment>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_Assignment()
    {
        var id = Guid.NewGuid();

        _currentUserMock.Setup(x => x.GetRole())
            .Returns(UserRole.Admin);
        
        _repoMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(new Assignment
            {
                Id = id,
                Title = "Test",
                Text = "Text",
                RequiresSubmission = true
            });

        var service = CreateService();

        var result = await service.GetByIdAsync(id);

        result.Id.Should().Be(id);
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_Only_NotNull_Fields()
    {
        var id = Guid.NewGuid();

        _currentUserMock.Setup(x => x.GetRole())
            .Returns(UserRole.Admin);
        
        var assignment = new Assignment
        {
            Id = id,
            Title = "Old title",
            Text = "Old text",
            RequiresSubmission = false
        };

        _repoMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(assignment);

        var dto = new UpdateAssignmentRequest
        {
            Title = "New title"
        };

        var service = CreateService();

        var result = await service.UpdateAsync(id, dto);

        result.Title.Should().Be("New title");
        result.Text.Should().Be("Old text");

        _repoMock.Verify(r => r.UpdateAsync(assignment), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_Should_Delete_Assignment()
    {
        var id = Guid.NewGuid();

        _currentUserMock.Setup(x => x.GetRole())
            .Returns(UserRole.Admin);
        
        var assignment = new Assignment { Id = id };

        _repoMock.Setup(r => r.GetByIdAsync(id))
            .ReturnsAsync(assignment);

        var service = CreateService();

        await service.DeleteAsync(id);

        _repoMock.Verify(r => r.DeleteAsync(assignment), Times.Once);
    }
}