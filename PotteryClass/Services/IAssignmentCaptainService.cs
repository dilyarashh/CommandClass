using PotteryClass.Data.DTOs;

namespace PotteryClass.Services;

public interface IAssignmentCaptainService
{
    Task<List<AssignmentCaptainDto>> GetByAssignmentAsync(Guid assignmentId);
    Task SelfAssignAsync(Guid assignmentId);
    Task WithdrawSelfAsync(Guid assignmentId);
}
