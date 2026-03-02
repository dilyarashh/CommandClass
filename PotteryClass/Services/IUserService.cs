using PotteryClass.Data.DTOs;
using PotteryClass.Data.Entities;

namespace PotteryClass.Services;

public interface IUserService
{
    Task<User> CreateUserAsync(RegistrationRequest dto);
}