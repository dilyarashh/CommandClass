using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotteryClass.Data.DTOs;
using PotteryClass.Services;

namespace PotteryClass.Controllers;

[ApiController]
[Route("api/assignments")]
public class CriterionGroupsController(ICriterionGroupService service) : ControllerBase
{
    /// <summary>
    /// Создать группу критериев у задания
    /// </summary>
    [Authorize]
    [HttpPost("{assignmentId:guid}/criterion-groups")]
    [ProducesResponseType(typeof(CriterionGroupDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CriterionGroupDto>> Create(
        Guid assignmentId,
        [FromBody] CreateCriterionGroupRequest request)
    {
        var result = await service.CreateAsync(assignmentId, request);
        return Ok(result);
    }

    /// <summary>
    /// Получить группы критериев задания
    /// </summary>
    [Authorize]
    [HttpGet("{assignmentId:guid}/criterion-groups")]
    [ProducesResponseType(typeof(List<CriterionGroupDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CriterionGroupDto>>> GetByAssignment(Guid assignmentId)
    {
        var result = await service.GetByAssignmentAsync(assignmentId);
        return Ok(result);
    }

    /// <summary>
    /// Обновить группу критериев
    /// </summary>
    [Authorize]
    [HttpPut("criterion-groups/{criterionGroupId:guid}")]
    [ProducesResponseType(typeof(CriterionGroupDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CriterionGroupDto>> Update(
        Guid criterionGroupId,
        [FromBody] UpdateCriterionGroupRequest request)
    {
        var result = await service.UpdateAsync(criterionGroupId, request);
        return Ok(result);
    }

    /// <summary>
    /// Удалить группу критериев
    /// </summary>
    [Authorize]
    [HttpDelete("criterion-groups/{criterionGroupId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid criterionGroupId)
    {
        await service.DeleteAsync(criterionGroupId);
        return NoContent();
    }
}
