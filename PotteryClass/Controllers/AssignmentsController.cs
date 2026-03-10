using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotteryClass.Data.DTOs;
using PotteryClass.Services;

namespace PotteryClass.Controllers;

[ApiController]
[Route("api/assignments")]
public class AssignmentsController(IAssignmentService service) : ControllerBase
{
    /// <summary>
    /// Создать задание
    /// </summary>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(AssignmentDto), 200)]
    public async Task<ActionResult<AssignmentDto>> Create(
        [FromBody] CreateAssignmentRequest dto)
    {
        var assignment = await service.CreateAsync(dto);
        return Ok(assignment);
    }

    /// <summary>
    /// Получить задание
    /// </summary>
    [Authorize]
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AssignmentDto), 200)]
    public async Task<ActionResult<AssignmentDto>> Get(Guid id)
    {
        var assignment = await service.GetByIdAsync(id);
        return Ok(assignment);
    }

    /// <summary>
    /// Обновить задание
    /// </summary>
    [Authorize]
    [HttpPatch("{id}")]
    [ProducesResponseType(typeof(AssignmentDto), 200)]
    public async Task<ActionResult<AssignmentDto>> Update(
        Guid id,
        [FromBody] UpdateAssignmentRequest dto)
    {
        var assignment = await service.UpdateAsync(id, dto);
        return Ok(assignment);
    }

    /// <summary>
    /// Удалить задание
    /// </summary>
    [Authorize]
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await service.DeleteAsync(id);
        return NoContent();
    }
}