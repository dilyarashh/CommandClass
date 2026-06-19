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
    /// Get assignment grading rules
    /// </summary>
    [Authorize]
    [HttpGet("{id:guid}/grading-rules")]
    [ProducesResponseType(typeof(AssignmentGradingRulesDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AssignmentGradingRulesDto>> GetGradingRules(Guid id)
    {
        var result = await service.GetGradingRulesAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Update assignment grading rules
    /// </summary>
    [Authorize]
    [HttpPut("{id:guid}/grading-rules")]
    [HttpPatch("{id:guid}/grading-rules")]
    [ProducesResponseType(typeof(AssignmentGradingRulesDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AssignmentGradingRulesDto>> UpdateGradingRules(
        Guid id,
        [FromBody] AssignmentGradingRulesDto dto)
    {
        var result = await service.UpdateGradingRulesAsync(id, dto);
        return Ok(result);
    }

    /// <summary>
    /// Изменить видимость задания для студентов
    /// </summary>
    [Authorize]
    [HttpPatch("{id}/visibility")]
    public async Task<IActionResult> UpdateVisibility(
        Guid id,
        [FromBody] UpdateAssignmentVisibilityRequest dto)
    {
        await service.UpdateVisibilityAsync(id, dto.IsVisible);
        return NoContent();
    }

    /// <summary>
    /// Изменить настройки peer-review задания
    /// </summary>
    [Authorize]
    [HttpPatch("{id}/peer-review")]
    [ProducesResponseType(typeof(AssignmentDto), 200)]
    public async Task<ActionResult<AssignmentDto>> UpdatePeerReview(
        Guid id,
        [FromBody] UpdateAssignmentPeerReviewRequest dto)
    {
        var assignment = await service.UpdatePeerReviewAsync(id, dto);
        return Ok(assignment);
    }

    /// <summary>
    /// Получить назначения peer-review задания
    /// </summary>
    [Authorize]
    [HttpGet("{id}/peer-review/assignments")]
    [ProducesResponseType(typeof(PeerReviewAssignmentResultDto), 200)]
    public async Task<ActionResult<PeerReviewAssignmentResultDto>> GetPeerReviewAssignments(Guid id)
    {
        var result = await service.GetPeerReviewAssignmentsAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Сформировать назначения peer-review задания
    /// </summary>
    [Authorize]
    [HttpPost("{id}/peer-review/assignments/generate")]
    [ProducesResponseType(typeof(PeerReviewAssignmentResultDto), 200)]
    public async Task<ActionResult<PeerReviewAssignmentResultDto>> GeneratePeerReviewAssignments(Guid id)
    {
        var result = await service.GeneratePeerReviewAssignmentsAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Получить форму peer-review текущего студента
    /// </summary>
    [Authorize]
    [HttpGet("{id}/peer-review/my-form")]
    [ProducesResponseType(typeof(PeerReviewMyFormDto), 200)]
    public async Task<ActionResult<PeerReviewMyFormDto>> GetMyPeerReviewForm(Guid id)
    {
        var result = await service.GetMyPeerReviewFormAsync(id);
        return Ok(result);
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

    /// <summary>
    /// Получить задания курса с учетом доступности для текущего пользователя
    /// </summary>
    [Authorize]
    [HttpGet("/api/courses/{courseId}/assignments/visible")]
    [ProducesResponseType(typeof(PagedResult<AssignmentDto>), 200)]
    public async Task<ActionResult<PagedResult<AssignmentDto>>> GetVisibleCourseAssignments(
        Guid courseId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await service.GetVisibleCourseAssignmentsAsync(courseId, page, pageSize);
        return Ok(result);
    }
}
