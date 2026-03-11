using FluentAssertions;
using Xunit;
using Moq;
using PotteryClass.Data.DTOs;
using PotteryClass.Data.Entities;
using PotteryClass.Data.Entities.Enums;
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


    /// Тесты на получение списка комментариев

    [Fact]
    public async Task GetComments_WhenAssignmentNotExists_ThrowsNotFound()
    {
        var repo = new Mock<ICommentRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var assignmentId = Guid.NewGuid();

        repo.Setup(x => x.AssignmentExistsAsync(assignmentId))
            .ReturnsAsync(false);

        var service = new CommentService(repo.Object, currentUser.Object);

        Func<Task> act = () => service.GetCommentsAsync(assignmentId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task GetComments_WhenAssignmentExists_ReturnsComments()
    {
        var repo = new Mock<ICommentRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var assignmentId = Guid.NewGuid();

        repo.Setup(x => x.AssignmentExistsAsync(assignmentId))
            .ReturnsAsync(true);

        repo.Setup(x => x.GetAssignmentCommentsAsync(assignmentId))
            .ReturnsAsync(new List<Comment>
            {
            new Comment
            {
                Id = Guid.NewGuid(),
                Text = "Test comment",
                Created = DateTime.UtcNow,
                User = new User
                {
                    Id = Guid.NewGuid(),
                    FirstName = "John",
                    LastName = "Doe"
                }
            }
            });

        var service = new CommentService(repo.Object, currentUser.Object);

        var result = await service.GetCommentsAsync(assignmentId);

        result.Should().HaveCount(1);
        result.First().Text.Should().Be("Test comment");
    }


    /// Тесты на удаление комментария

    [Fact]
    public async Task DeleteComment_WhenCommentNotExists_ThrowsNotFound()
    {
        var repo = new Mock<ICommentRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var commentId = Guid.NewGuid();

        repo.Setup(x => x.GetByIdAsync(commentId))
            .ReturnsAsync((Comment?)null);

        var service = new CommentService(repo.Object, currentUser.Object);

        Func<Task> act = () => service.DeleteCommentAsync(commentId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteComment_WhenUserIsAuthor_DeletesComment()
    {
        var repo = new Mock<ICommentRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var userId = Guid.NewGuid();
        var commentId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId()).Returns(userId);

        repo.Setup(x => x.GetByIdAsync(commentId))
            .ReturnsAsync(new Comment
            {
                Id = commentId,
                UserId = userId
            });

        var service = new CommentService(repo.Object, currentUser.Object);

        await service.DeleteCommentAsync(commentId);

        repo.Verify(x => x.Delete(It.IsAny<Comment>()), Times.Once);
        repo.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteComment_WhenUserIsAdmin_DeletesComment()
    {
        var repo = new Mock<ICommentRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var commentId = Guid.NewGuid();

        currentUser.Setup(x => x.GetRole()).Returns(UserRole.Admin);

        repo.Setup(x => x.GetByIdAsync(commentId))
            .ReturnsAsync(new Comment
            {
                Id = commentId,
                UserId = Guid.NewGuid()
            });

        var service = new CommentService(repo.Object, currentUser.Object);

        await service.DeleteCommentAsync(commentId);

        repo.Verify(x => x.Delete(It.IsAny<Comment>()), Times.Once);
        repo.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteComment_WhenUserNotAuthorAndNotAdmin_ThrowsForbidden()
    {
        var repo = new Mock<ICommentRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var userId = Guid.NewGuid();
        var commentId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId()).Returns(userId);
        currentUser.Setup(x => x.GetRole()).Returns(UserRole.Teacher);

        repo.Setup(x => x.GetByIdAsync(commentId))
            .ReturnsAsync(new Comment
            {
                Id = commentId,
                UserId = Guid.NewGuid()
            });

        var service = new CommentService(repo.Object, currentUser.Object);

        Func<Task> act = () => service.DeleteCommentAsync(commentId);

        await act.Should().ThrowAsync<ForbiddenException>();
    }


    /// Тесты на редактирование комментария

    [Fact]
    public async Task UpdateComment_WhenCommentNotExists_ThrowsNotFound()
    {
        var repo = new Mock<ICommentRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var commentId = Guid.NewGuid();
        var request = new UpdateCommentRequest { Text = "Updated text" };

        repo.Setup(x => x.GetByIdAsync(commentId))
            .ReturnsAsync((Comment?)null);

        var service = new CommentService(repo.Object, currentUser.Object);

        Func<Task> act = () => service.UpdateCommentAsync(commentId, request);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateComment_WhenUserIsNotAuthor_ThrowsForbidden()
    {
        var repo = new Mock<ICommentRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var commentId = Guid.NewGuid();
        var currentUserId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId()).Returns(currentUserId);

        repo.Setup(x => x.GetByIdAsync(commentId))
            .ReturnsAsync(new Comment
            {
                Id = commentId,
                UserId = authorId,
                Text = "Old text"
            });

        var service = new CommentService(repo.Object, currentUser.Object);

        Func<Task> act = () => service.UpdateCommentAsync(
            commentId,
            new UpdateCommentRequest { Text = "Updated text" });

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task UpdateComment_WhenTextEmpty_ThrowsBadRequest()
    {
        var repo = new Mock<ICommentRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var commentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId()).Returns(userId);

        repo.Setup(x => x.GetByIdAsync(commentId))
            .ReturnsAsync(new Comment
            {
                Id = commentId,
                UserId = userId,
                Text = "Old text"
            });

        var service = new CommentService(repo.Object, currentUser.Object);

        Func<Task> act = () => service.UpdateCommentAsync(
            commentId,
            new UpdateCommentRequest { Text = "" });

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task UpdateComment_WhenUserIsAuthor_UpdatesComment()
    {
        var repo = new Mock<ICommentRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var commentId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var comment = new Comment
        {
            Id = commentId,
            UserId = userId,
            AssignmentId = Guid.NewGuid(),
            Text = "Old text",
            Created = DateTime.UtcNow.AddMinutes(-10)
        };

        currentUser.Setup(x => x.GetUserId()).Returns(userId);

        repo.Setup(x => x.GetByIdAsync(commentId))
            .ReturnsAsync(comment);

        repo.Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var service = new CommentService(repo.Object, currentUser.Object);

        var result = await service.UpdateCommentAsync(
            commentId,
            new UpdateCommentRequest { Text = "Updated text" });

        result.Id.Should().Be(commentId);
        result.Text.Should().Be("Updated text");
        result.UserId.Should().Be(userId);

        comment.Text.Should().Be("Updated text");

        repo.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task GetComments_WhenCommentsExist_ReturnsCommentsOrderedByCreatedDesc()
    {
        var repo = new Mock<ICommentRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var assignmentId = Guid.NewGuid();
        var older = DateTime.UtcNow.AddMinutes(-10);
        var newer = DateTime.UtcNow;

        repo.Setup(x => x.AssignmentExistsAsync(assignmentId))
            .ReturnsAsync(true);

        repo.Setup(x => x.GetAssignmentCommentsAsync(assignmentId))
            .ReturnsAsync(new List<Comment>
            {
            new Comment
            {
                Id = Guid.NewGuid(),
                Text = "New comment",
                Created = newer,
                UserId = Guid.NewGuid(),
                User = new User
                {
                    FirstName = "John",
                    LastName = "Doe"
                }
            },
            new Comment
            {
                Id = Guid.NewGuid(),
                Text = "Old comment",
                Created = older,
                UserId = Guid.NewGuid(),
                User = new User
                {
                    FirstName = "Jane",
                    LastName = "Smith"
                }
            }
            });

        var service = new CommentService(repo.Object, currentUser.Object);

        var result = await service.GetCommentsAsync(assignmentId);

        result.Should().HaveCount(2);
        result[0].Text.Should().Be("New comment");
        result[1].Text.Should().Be("Old comment");
    }
}