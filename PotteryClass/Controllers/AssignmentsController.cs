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
    public async Task<ActionResult<AssignmentFileDto>> UploadFile(
        Guid id,
        [FromForm] AssignmentFileFormRequest dto)
    {
        var result = await service.AddFileAsync(id, dto);
        return Ok(result);
    }

    /// <summary>
    /// Удаление файла из задания
    /// </summary>
    [Authorize]
    [HttpDelete("{assignmentId:guid}/files/{fileId:guid}")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> DeleteFile(Guid assignmentId, Guid fileId)
    {
        await service.DeleteFileAsync(assignmentId, fileId);
        return NoContent();
    }
}