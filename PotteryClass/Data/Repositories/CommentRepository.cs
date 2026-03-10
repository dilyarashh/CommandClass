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

    public Task<bool> AssignmentExistsAsync(Guid assignmentId)
        => db.Assignments.AnyAsync(x => x.Id == assignmentId);

    public Task<List<Comment>> GetAssignmentCommentsAsync(Guid assignmentId)
        => db.Comments
            .Where(x => x.AssignmentId == assignmentId)
            .Include(x => x.User)
            .OrderBy(x => x.Created)
            .ToListAsync();
}