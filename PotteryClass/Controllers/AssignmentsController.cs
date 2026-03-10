using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotteryClass.Data.DTOs;
using PotteryClass.Data.Entities.Enums;
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
    
    /// <summary>
    /// Добавление файла к заданию
    /// </summary>
    [HttpPost("{id}/files")]
    [Authorize]
    [RequestSizeLimit(500_000_000)]
    public async Task<ActionResult<List<AssignmentFileDto>>> UploadFiles(
        Guid id,
        [FromForm] List<IFormFile> files)
    {
        var dtos = files.Select(f => new AssignmentFileFormRequest { File = f }).ToList();
        var request = new AssignmentFilesFormRequest { Files = dtos };
        var result = await service.AddFileAsync(id, request);
        return Ok(result);
    }

    /// <summary>
    /// Удаление файла из задания
    /// </summary>
    [HttpDelete("{assignmentId}/files")]
    [Authorize]
    public async Task<IActionResult> DeleteFiles(
        Guid assignmentId,
        [FromBody] List<Guid> fileIds)
    {
        await service.DeleteFileAsync(assignmentId, fileIds);
        return NoContent();
    }
    
    /// <summary>
    /// Получить задания курса
    /// </summary>
    [Authorize]
    [HttpGet("/api/courses/{courseId}/assignments")]
    [ProducesResponseType(typeof(PagedResult<AssignmentDto>), 200)]
    public async Task<ActionResult<PagedResult<AssignmentDto>>> GetCourseAssignments(
        Guid courseId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await service.GetCourseAssignmentsAsync(courseId, page, pageSize);
        return Ok(result);
    }
}