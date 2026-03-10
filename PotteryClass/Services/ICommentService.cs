using PotteryClass.Data.DTOs;

namespace PotteryClass.Services;

public interface ICommentService
{
    Task<CommentDto> CreateCommentAsync(Guid assignmentId, CreateCommentRequest request);

    Task<List<CommentDto>> GetCommentsAsync(Guid assignmentId);
}