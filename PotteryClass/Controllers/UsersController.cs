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
    
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), 200)]
    public async Task<ActionResult<UserDto>> GetMe()
    {
        var user = await service.GetCurrentUserAsync();
        return Ok(user);
    }
    
    [Authorize]
    [HttpPatch("me")]
    [ProducesResponseType(typeof(UserDto), 200)]
    public async Task<ActionResult<UserDto>> UpdateMe([FromBody] UpdateProfileRequest dto)
    {
        var user = await service.UpdateProfileAsync(dto);
        return Ok(user);
    }
    
    [Authorize]
    [HttpDelete("me")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteMe()
    {
        await service.DeleteCurrentUserAsync();
        return NoContent();
    }
}