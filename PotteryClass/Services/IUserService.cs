using PotteryClass.Data.DTOs;
using PotteryClass.Data.Entities;

namespace PotteryClass.Services;

public interface IUserService
{
    Task<User> CreateUserAsync(RegistrationRequest dto);
    Task<UserDto> GetCurrentUserAsync();
    Task<UserDto> UpdateProfileAsync(UpdateProfileRequest dto);
    Task DeleteCurrentUserAsync();
    Task<UserDto> GetByIdAsync(Guid id);
    Task<PagedResult<UserDto>> GetAllAsync(UsersQuery query);
    Task<UserRoleDto> GetActualRoleByIdAsync(Guid id);
}