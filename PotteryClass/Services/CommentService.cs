using PotteryClass.Data.DTOs;
using PotteryClass.Data.Repositories;
using PotteryClass.Infrastructure.Auth;

namespace PotteryClass.Services;

public class CommentService(ICommentRepository repo, ICurrentUser currentUser) : ICommentService
{
    public Task<CommentDto> CreateCommentAsync(Guid assignmentId, CreateCommentRequest request)
    {
        throw new NotImplementedException();
    }
}