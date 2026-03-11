using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotteryClass.Data.DTOs;
using PotteryClass.Services;

namespace PotteryClass.Controllers;

[ApiController]
[Route("api/submissions")]
public class SubmissionsController(ISubmissionService service) : ControllerBase
{
    /// <summary>
    /// Отправить решение задания
    /// </summary>
    [Authorize]
    [HttpPost("{assignmentId}/files")]
    [RequestSizeLimit(500_000_000)]
    public async Task<ActionResult<SubmissionDto>> Submit(
        Guid assignmentId,
        [FromForm] List<IFormFile> files)
    {
        var dtos = files.Select(f => new SubmissionFileFormRequest { File = f }).ToList();
        var request = new SubmissionFilesFormRequest { Files = dtos };
        var result = await service.SubmitAsync(assignmentId, request);
        return Ok(result);
    }

    /// <summary>
    /// Удалить файлы решения
    /// </summary>
    [Authorize]
    [HttpDelete("{submissionId}/files")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteFiles(
        Guid submissionId,
        [FromBody] List<Guid> fileIds)
    {
        await service.DeleteFilesAsync(submissionId, fileIds);
        return NoContent();
    }
}