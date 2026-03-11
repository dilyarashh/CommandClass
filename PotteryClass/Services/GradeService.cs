using PotteryClass.Data.DTOs;
using PotteryClass.Data.Repositories;
using PotteryClass.Infrastructure.Auth;

namespace PotteryClass.Services;

public class GradeService(
    IAssignmentRepository assignmentRepo,
    IGradeRepository gradeRepo,
    ICourseRepository courseRepo,
    ICurrentUser currentUser)
    : IGradeService
{
    public async Task<GradeDto> CreateGradeAsync(Guid assignmentId, CreateGradeRequest dto)
    {
        throw new NotImplementedException();
    }
}