using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotteryClass.Data.DTOs;
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
}