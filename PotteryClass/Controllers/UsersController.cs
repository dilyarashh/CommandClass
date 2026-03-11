using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotteryClass.Data.DTOs;
using PotteryClass.Data.Entities;
using PotteryClass.Services;

namespace PotteryClass.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController(IUserService service) : ControllerBase
{
    /// <summary>
    /// Зарегистрироваться
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), 200)]
    public async Task<ActionResult<Guid>> Create([FromBody] RegistrationRequest dto)
    {
        var user = await service.CreateUserAsync(dto);
        return Ok(user.Id);
    }
    
    /// <summary>
    /// Получить информацию о себе
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), 200)]
    public async Task<ActionResult<UserDto>> GetMe()
    {
        var user = await service.GetCurrentUserAsync();
        return Ok(user);
    }
    
    /// <summary>
    /// Редактировать профиль
    /// </summary>
    [Authorize]
    [HttpPatch("me")]
    [ProducesResponseType(typeof(UserDto), 200)]
    public async Task<ActionResult<UserDto>> UpdateMe([FromBody] UpdateProfileRequest dto)
    {
        var user = await service.UpdateProfileAsync(dto);
        return Ok(user);
    }
    
    /// <summary>
    /// Удалить профиль
    /// </summary>
    [Authorize]
    [HttpDelete("me")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteMe()
    {
        await service.DeleteCurrentUserAsync();
        return NoContent();
    }
    
    /// <summary>
    /// Получить пользователя по айди
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserDto), 200)]
    public async Task<ActionResult<UserDto>> GetById(Guid id)
    {
        var user = await service.GetByIdAsync(id);
        return Ok(user);
    }
    
    /// <summary>
    /// Получить пользователей
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<UserDto>), 200)]
    public async Task<ActionResult<PagedResult<UserDto>>> GetAll(
        [FromQuery] UsersQuery query)
    {
        var users = await service.GetAllAsync(query);
        return Ok(users);
    }

    /// <summary>
    /// Получить фактическую роль текущего пользователя
    /// </summary>
    [Authorize]
    [HttpGet("me/role")]
    [ProducesResponseType(typeof(UserRoleDto), 200)]
    public async Task<ActionResult<UserRoleDto>> GetMyActualRole()
    {
        var role = await service.GetActualRoleAsync();
        return Ok(role);
    }
}