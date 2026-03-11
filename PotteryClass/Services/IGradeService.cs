using PotteryClass.Data.DTOs;

namespace PotteryClass.Services;

public interface IGradeService
{
    Task<GradeDto> CreateGradeAsync(Guid assignmentId, CreateGradeRequest dto);
}