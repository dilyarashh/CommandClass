using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotteryClass.Data.DTOs;
using PotteryClass.Services;

namespace PotteryClass.Controllers;

[ApiController]
[Route("api")]
public class CriteriaController(ICriterionService service) : ControllerBase
{
    /// <summary>
    /// Создать критерий внутри группы критериев
    /// </summary>
    [Authorize]
    [HttpPost("criterion-groups/{criterionGroupId:guid}/criteria")]
    [ProducesResponseType(typeof(CriterionDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CriterionDto>> Create(
        Guid criterionGroupId,
        [FromBody] CreateCriterionRequest request)
    {
        var result = await service.CreateAsync(criterionGroupId, request);
        return Ok(result);
    }

    /// <summary>
    /// Получить критерии группы
    /// </summary>
    [Authorize]
    [HttpGet("criterion-groups/{criterionGroupId:guid}/criteria")]
    [ProducesResponseType(typeof(List<CriterionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CriterionDto>>> GetByCriterionGroup(Guid criterionGroupId)
    {
        var result = await service.GetByCriterionGroupAsync(criterionGroupId);
        return Ok(result);
    }

    /// <summary>
    /// Обновить критерий
    /// </summary>
    [Authorize]
    [HttpPut("criteria/{criterionId:guid}")]
    [ProducesResponseType(typeof(CriterionDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<CriterionDto>> Update(
        Guid criterionId,
        [FromBody] UpdateCriterionRequest request)
    {
        var result = await service.UpdateAsync(criterionId, request);
        return Ok(result);
    }

    /// <summary>
    /// Удалить критерий
    /// </summary>
    [Authorize]
    [HttpDelete("criteria/{criterionId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid criterionId)
    {
        await service.DeleteAsync(criterionId);
        return NoContent();
    }
}
