using Microsoft.EntityFrameworkCore;
using PotteryClass.Data.Entities;

namespace PotteryClass.Data.Repositories;

public class AssignmentRepository(AppDbContext db) : IAssignmentRepository
{
    public async Task AddAsync(Assignment assignment)
    {
        db.Assignments.Add(assignment);
        await db.SaveChangesAsync();
    }

    public async Task<Assignment?> GetByIdAsync(Guid id)
    {
        return await db.Assignments
            .Include(a => a.Files)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task UpdateAsync(Assignment assignment)
    {
        db.Assignments.Update(assignment);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Assignment assignment)
    {
        db.Assignments.Remove(assignment);
        await db.SaveChangesAsync();
    }
    
    public async Task AddFileAsync(AssignmentFile file)
    {
        db.AssignmentFiles.Add(file);
        await db.SaveChangesAsync();
    }
    
    public async Task<(List<Assignment>, int)> GetByCourseAsync(
        Guid courseId,
        int page,
        int pageSize)
    {
        var query = db.Assignments
            .Where(a => a.CourseId == courseId)
            .Include(a => a.Files)
            .OrderByDescending(a => a.Created);

        var total = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }
}