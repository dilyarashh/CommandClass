using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotteryClass.Data.DTOs;
using PotteryClass.Services;

namespace PotteryClass.Controllers;

[ApiController]
[Route("api/assignments")]
public class AssignmentCaptainsController(IAssignmentCaptainService service) : ControllerBase
{
    /// <summary>
    /// Получить капитанов задания
    /// </summary>
    [Authorize]
    [HttpGet("{assignmentId:guid}/captains")]
    public async Task<ActionResult<List<AssignmentCaptainDto>>> GetCaptains(Guid assignmentId)
    {
        var result = await service.GetByAssignmentAsync(assignmentId);
        return Ok(result);
    }

    /// <summary>
    /// Выбрать себя капитаном задания
    /// </summary>
    [Authorize]
    [HttpPost("{assignmentId:guid}/captains/self")]
    public async Task<IActionResult> SelfAssign(Guid assignmentId)
    {
        await service.SelfAssignAsync(assignmentId);
        return NoContent();
    }

    /// <summary>
    /// Снять себя с роли капитана задания
    /// </summary>
    [Authorize]
    [HttpDelete("{assignmentId:guid}/captains/self")]
    public async Task<IActionResult> WithdrawSelf(Guid assignmentId)
    {
        await service.WithdrawSelfAsync(assignmentId);
        return NoContent();
    }
}
