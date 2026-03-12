using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotteryClass.Data.DTOs;
using PotteryClass.Services;

namespace PotteryClass.Controllers;

[ApiController]
[Route("api/submissions")]
public class GradesController(IGradeService service) : ControllerBase
{
    /// <summary>
    /// Поставить или обновить оценку за решение
    /// </summary>
    [Authorize]
    [HttpPut("{submissionId:guid}/grade")]
    [ProducesResponseType(typeof(SubmissionGradeDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SubmissionGradeDto>> SetGrade(
        Guid submissionId,
        [FromBody] SetSubmissionGradeRequest request)
    {
        var result = await service.SetGradeAsync(submissionId, request);

        return Ok(result);
    }

    /// <summary>
    /// Удалить оценку у решения
    /// </summary>
    [Authorize]
    [HttpDelete("{submissionId:guid}/grade")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteGrade(Guid submissionId)
    {
        await service.DeleteGradeAsync(submissionId);

        return NoContent();
    }

    /// <summary>
    /// Получить список оценок студентов на курсе
    /// </summary>
    [Authorize]
    [HttpGet("/api/courses/{courseId:guid}/grades")]
    [ProducesResponseType(typeof(List<CourseStudentGradeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<CourseStudentGradeDto>>> GetCourseGrades(Guid courseId)
    {
        var result = await service.GetCourseGradesAsync(courseId);

        return Ok(result);
    }
}