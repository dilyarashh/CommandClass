using PotteryClass.Data.DTOs;
using PotteryClass.Data.Entities;
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
        throw new NotImplementedException();
    }
}