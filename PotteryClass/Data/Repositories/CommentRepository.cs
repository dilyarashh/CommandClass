using Microsoft.EntityFrameworkCore;
using PotteryClass.Data.Entities;

namespace PotteryClass.Data.Repositories;

public class CommentRepository(AppDbContext db) : ICommentRepository
{
    public Task<Assignment?> GetAssignmentAsync(Guid assignmentId)
        => db.Assignments
            .FirstOrDefaultAsync(x => x.Id == assignmentId);

    public async Task AddAsync(Comment comment)
        => await db.Comments.AddAsync(comment);

    public Task SaveChangesAsync()
        => db.SaveChangesAsync();
}