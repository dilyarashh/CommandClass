using PotteryClass.Data.Entities;

namespace PotteryClass.Data.Repositories;

public interface IAssignmentTeamRepository
{
    Task<AssignmentTeam?> GetByIdAsync(Guid teamId);
    Task<List<AssignmentTeam>> GetByAssignmentAsync(Guid assignmentId);
    Task<AssignmentTeam?> GetCaptainTeamAsync(Guid assignmentId, Guid captainUserId);
    Task<bool> IsStudentInAssignmentTeamsAsync(Guid assignmentId, Guid studentId);
    Task<bool> HasCaptainTeamAsync(Guid assignmentId, Guid captainUserId);
    Task AddAsync(AssignmentTeam team);
    Task SaveChangesAsync();
}
