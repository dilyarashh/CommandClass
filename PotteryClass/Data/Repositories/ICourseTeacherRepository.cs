namespace PotteryClass.Data.Repositories;

public interface ICourseTeacherRepository
{
    Task<bool> IsTeacherAsync(Guid courseId, Guid userId);
}