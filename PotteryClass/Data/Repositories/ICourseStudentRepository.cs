namespace PotteryClass.Data.Repositories;

public interface ICourseStudentRepository
{
    Task<bool> IsStudentAsync(Guid courseId, Guid userId);
    Task<List<Guid>> GetActiveStudentIdsAsync(Guid courseId);
}
