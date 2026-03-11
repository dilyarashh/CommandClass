using PotteryClass.Data.DTOs;
using PotteryClass.Data.Entities;
using PotteryClass.Data.Entities.Enums;
using PotteryClass.Data.Repositories;
using PotteryClass.Infrastructure.Auth;
using PotteryClass.Infrastructure.Errors.Exceptions;

namespace PotteryClass.Services;

public class CommentService(ICommentRepository repo, ICurrentUser currentUser) : ICommentService
{
    public async Task<CommentDto> CreateCommentAsync(Guid assignmentId, CreateCommentRequest request)
    {
        var assignment = await repo.GetAssignmentAsync(assignmentId);

        if (assignment == null)
            throw new NotFoundException("Задание не найдено");

        if (string.IsNullOrWhiteSpace(request.Text))
            throw new BadRequestException("Текст комментария не может быть пустым");

        var userId = currentUser.GetUserId();

        var comment = new Comment
        {
            Id = Guid.NewGuid(),
            AssignmentId = assignmentId,
            UserId = userId,
            Text = request.Text,
            Created = DateTime.UtcNow
        };

        await repo.AddAsync(comment);
        await repo.SaveChangesAsync();

        return new CommentDto
        {
            Id = comment.Id,
            AssignmentId = comment.AssignmentId,
            UserId = comment.UserId,
            Text = comment.Text,
            Created = comment.Created
        };
    }

    public async Task<List<CommentDto>> GetCommentsAsync(Guid assignmentId)
    {
        var assignmentExists = await repo.AssignmentExistsAsync(assignmentId);

        if (!assignmentExists)
            throw new NotFoundException("Задание не найдено");

        var comments = await repo.GetAssignmentCommentsAsync(assignmentId);

        return comments
            .Select(x => new CommentDto
            {
                Id = x.Id,
                AssignmentId = x.AssignmentId,
                Text = x.Text,
                Created = x.Created,
                UserId = x.UserId,
                UserName = $"{x.User.FirstName} {x.User.LastName}"
            })
            .ToList();
    }

    public async Task DeleteCommentAsync(Guid commentId)
    {
        var comment = await repo.GetByIdAsync(commentId);

        if (comment == null)
            throw new NotFoundException("Комментарий не найден");

        var currentUserId = currentUser.GetUserId();
        var currentRole = currentUser.GetRole();

        var isAuthor = comment.UserId == currentUserId;
        var isAdmin = currentRole == UserRole.Admin;

        if (!isAuthor && !isAdmin)
            throw new ForbiddenException("Вы не можете удалить этот комментарий (вы не его автор и не админ)");

        repo.Delete(comment);
        await repo.SaveChangesAsync();
    }
}