using FluentAssertions;
using Xunit;
using Moq;
using PotteryClass.Data.DTOs;
using PotteryClass.Data.Entities;
using PotteryClass.Data.Repositories;
using PotteryClass.Infrastructure.Auth;
using PotteryClass.Infrastructure.Errors.Exceptions;
using PotteryClass.Services;

namespace PotteryClassTests;

public class CommentServiceTests
{
    /// Тесты на создание комментария

    [Fact]
    public async Task CreateComment_WhenAssignmentNotExists_ThrowsNotFound()
    {
        var repo = new Mock<ICommentRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var assignmentId = Guid.NewGuid();

        repo.Setup(x => x.GetAssignmentAsync(assignmentId))
            .ReturnsAsync((Assignment?)null);

        var service = new CommentService(repo.Object, currentUser.Object);

        Func<Task> act = () => service.CreateCommentAsync(
            assignmentId,
            new CreateCommentRequest { Text = "hello" });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateComment_WhenTextEmpty_ThrowsBadRequest()
    {
        var repo = new Mock<ICommentRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var assignmentId = Guid.NewGuid();

        repo.Setup(x => x.GetAssignmentAsync(assignmentId))
            .ReturnsAsync(new Assignment());

        var service = new CommentService(repo.Object, currentUser.Object);

        Func<Task> act = () => service.CreateCommentAsync(
            assignmentId,
            new CreateCommentRequest { Text = "" });

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task CreateComment_WhenValid_CreatesComment()
    {
        var repo = new Mock<ICommentRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var assignmentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId())
            .Returns(userId);

        repo.Setup(x => x.GetAssignmentAsync(assignmentId))
            .ReturnsAsync(new Assignment
            {
                Id = assignmentId
            });

        repo.Setup(x => x.AddAsync(It.IsAny<Comment>()))
            .Returns(Task.CompletedTask);

        repo.Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var service = new CommentService(repo.Object, currentUser.Object);

        var result = await service.CreateCommentAsync(
            assignmentId,
            new CreateCommentRequest { Text = "Hello world" });

        result.Text.Should().Be("Hello world");

        repo.Verify(x => x.AddAsync(It.IsAny<Comment>()), Times.Once);
        repo.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}