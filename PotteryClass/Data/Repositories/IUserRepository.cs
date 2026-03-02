using PotteryClass.Data.Entities;

namespace PotteryClass.Data.Repositories;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
}