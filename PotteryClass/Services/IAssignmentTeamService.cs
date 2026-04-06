using PotteryClass.Data.DTOs;

namespace PotteryClass.Services;

public interface IAssignmentTeamService
{
    Task<AssignmentTeamDto> CreateAsync(Guid assignmentId, CreateAssignmentTeamRequest request);
    Task<List<AssignmentTeamDto>> GetByAssignmentAsync(Guid assignmentId);
    Task AddMemberAsync(Guid teamId, Guid studentId);
    Task RemoveMemberAsync(Guid teamId, Guid studentId);
    Task JoinSelfAsync(Guid teamId);
    Task LeaveSelfAsync(Guid teamId);
    Task<List<AssignmentTeamDto>> DistributeRandomlyAsync(Guid assignmentId);
    Task<AssignmentManualDistributionDto> GetManualDistributionAsync(Guid assignmentId);
}
