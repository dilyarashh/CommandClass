using PotteryClass.Data.DTOs;

namespace PotteryClass.Services;

public interface ISubmissionService
{
    Task<SubmissionDto> SubmitAsync(Guid assignmentId, SubmissionFilesFormRequest dto);
    Task DeleteFilesAsync(Guid submissionId, List<Guid> fileIds);
    Task<List<SubmissionDto>> GetAssignmentSubmissionsAsync(Guid assignmentId);
    Task<SubmissionDto> GetByIdAsync(Guid submissionId);
    Task<SubmissionDto> GetMySubmissionAsync(Guid assignmentId);
}