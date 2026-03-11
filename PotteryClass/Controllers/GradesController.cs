using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotteryClass.Data.DTOs;
using PotteryClass.Services;

namespace PotteryClass.Controllers;

[ApiController]
[Route("api/assignments")]
public class GradesController(IGradeService service) : ControllerBase
{
    /// <summary>
    /// Поставить оценку за задание студенту
    /// </summary>
    [Authorize]
    [HttpPost("{assignmentId:guid}/grades")]
    [ProducesResponseType(typeof(GradeDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<GradeDto>> CreateGrade(Guid assignmentId, [FromBody] CreateGradeRequest request)
    {
        var result = await service.CreateGradeAsync(assignmentId, request);

        return Ok(result);
    }
}