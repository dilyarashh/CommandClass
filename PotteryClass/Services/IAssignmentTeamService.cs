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
    Task<AssignmentDraftStateDto> GetDraftStateAsync(Guid assignmentId);
    Task<CaptainTeamDto> GetCaptainTeamAsync(Guid assignmentId);
    Task<AssignmentDraftStateDto> StartDraftAsync(Guid assignmentId);
    Task<AssignmentDraftStateDto> DraftPickAsync(Guid assignmentId, Guid studentId);
    Task SelectFinalSubmissionAsync(Guid assignmentId, Guid submissionId);
    Task LockCompositionAsync(Guid assignmentId);
}
