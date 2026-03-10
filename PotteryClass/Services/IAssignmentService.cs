using PotteryClass.Data.DTOs;

namespace PotteryClass.Services;

public interface IAssignmentService
{
    Task<AssignmentDto> CreateAsync(CreateAssignmentRequest dto);
    Task<AssignmentDto> GetByIdAsync(Guid id);
    Task<AssignmentDto> UpdateAsync(Guid id, UpdateAssignmentRequest dto);
    Task DeleteAsync(Guid id);
    Task<AssignmentFileDto> AddFileAsync(Guid assignmentId, AssignmentFileFormRequest request);
    Task DeleteFileAsync(Guid assignmentId, Guid fileId);
    Task<PagedAssignmentResult<AssignmentDto>> GetCourseAssignmentsAsync(
        Guid courseId,
        int page,
        int pageSize);
}