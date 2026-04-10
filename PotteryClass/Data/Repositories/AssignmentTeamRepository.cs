using Microsoft.EntityFrameworkCore;
using PotteryClass.Data.Entities;

namespace PotteryClass.Data.Repositories;

public class AssignmentTeamRepository(AppDbContext db) : IAssignmentTeamRepository
{
    public Task<AssignmentTeam?> GetByIdAsync(Guid teamId)
        => db.AssignmentTeams
            .Include(x => x.Assignment)
            .Include(x => x.CaptainUser)
            .Include(x => x.FinalSubmission)
            .Include(x => x.Members)
            .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(x => x.Id == teamId);

    public Task<List<AssignmentTeam>> GetByAssignmentAsync(Guid assignmentId)
        => db.AssignmentTeams
            .Include(x => x.Assignment)
            .Include(x => x.CaptainUser)
            .Include(x => x.FinalSubmission)
            .Include(x => x.Members)
            .ThenInclude(x => x.User)
            .Where(x => x.AssignmentId == assignmentId)
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync();

    public Task<AssignmentTeam?> GetCaptainTeamAsync(Guid assignmentId, Guid captainUserId)
        => db.AssignmentTeams
            .Include(x => x.Assignment)
            .Include(x => x.CaptainUser)
            .Include(x => x.FinalSubmission)
            .Include(x => x.Members)
            .ThenInclude(x => x.User)
            .FirstOrDefaultAsync(x => x.AssignmentId == assignmentId && x.CaptainUserId == captainUserId);

    public Task<bool> IsStudentInAssignmentTeamsAsync(Guid assignmentId, Guid studentId)
        => db.AssignmentTeamMembers
            .AnyAsync(x => x.Team.AssignmentId == assignmentId && x.UserId == studentId);

    public Task<bool> HasCaptainTeamAsync(Guid assignmentId, Guid captainUserId)
        => db.AssignmentTeams
            .AnyAsync(x => x.AssignmentId == assignmentId && x.CaptainUserId == captainUserId);

    public async Task AddAsync(AssignmentTeam team)
        => await db.AssignmentTeams.AddAsync(team);

    public Task SaveChangesAsync()
        => db.SaveChangesAsync();
}
