using PotteryClass.Data.Entities;

namespace PotteryClass.Data.Repositories;

public interface ICommentRepository
{
    Task<Assignment?> GetAssignmentAsync(Guid assignmentId);

    Task AddAsync(Comment comment);

    Task SaveChangesAsync();
}