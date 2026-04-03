using PotteryClass.Data.DTOs;
using PotteryClass.Data.Entities;

namespace PotteryClass.Data.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task DeleteAsync(User user);
    Task<PagedResult<User>> GetAllAsync(UsersQuery query);
    Task<bool> IsTeacherAnywhereAsync(Guid userId);
    Task<HashSet<Guid>> GetTeacherIdsAsync(IEnumerable<Guid> userIds);
}
