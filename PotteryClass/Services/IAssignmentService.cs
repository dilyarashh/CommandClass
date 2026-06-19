using PotteryClass.Data.DTOs;

namespace PotteryClass.Services;

public interface IAssignmentService
{
    Task<AssignmentDto> CreateAsync(CreateAssignmentRequest dto);
    Task<AssignmentDto> GetByIdAsync(Guid id);
    Task<AssignmentDto> UpdateAsync(Guid id, UpdateAssignmentRequest dto);
    Task DeleteAsync(Guid id);
    Task<List<AssignmentFileDto>> AddFileAsync(Guid assignmentId, AssignmentFilesFormRequest request);
    Task DeleteFileAsync(Guid assignmentId, List<Guid> fileIds);
    Task<PagedAssignmentResult<AssignmentDto>> GetCourseAssignmentsAsync(
        Guid courseId,
        int page,
        int pageSize);
    Task<PagedAssignmentResult<AssignmentDto>> GetVisibleCourseAssignmentsAsync(
        Guid courseId,
        int page,
        int pageSize);
    Task UpdateVisibilityAsync(Guid id, bool isVisible);
    Task<AssignmentDto> UpdatePeerReviewAsync(Guid id, UpdateAssignmentPeerReviewRequest dto);
    Task<PeerReviewAssignmentResultDto> GetPeerReviewAssignmentsAsync(Guid assignmentId);
    Task<PeerReviewAssignmentResultDto> GeneratePeerReviewAssignmentsAsync(Guid assignmentId);
    Task<PeerReviewMyFormDto> GetMyPeerReviewFormAsync(Guid assignmentId);
    Task<AssignmentGradingRulesDto> GetGradingRulesAsync(Guid assignmentId);
    Task<AssignmentGradingRulesDto> UpdateGradingRulesAsync(Guid assignmentId, AssignmentGradingRulesDto dto);
}
