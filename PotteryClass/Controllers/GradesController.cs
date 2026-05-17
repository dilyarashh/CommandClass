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
    /// Получить форму проверки решения по критериям
    /// </summary>
    [Authorize]
    [HttpGet("{submissionId:guid}/assessment-form")]
    [ProducesResponseType(typeof(SubmissionAssessmentFormDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SubmissionAssessmentFormDto>> GetAssessmentForm(Guid submissionId)
    {
        var result = await service.GetAssessmentFormAsync(submissionId);

        return Ok(result);
    }

    /// <summary>
    /// Получить сохраненную проверку решения по критериям
    /// </summary>
    [Authorize]
    [HttpGet("{submissionId:guid}/assessment")]
    [ProducesResponseType(typeof(SubmissionAssessmentDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SubmissionAssessmentDto>> GetAssessment(Guid submissionId)
    {
        var result = await service.GetAssessmentAsync(submissionId);

        return Ok(result);
    }

    /// <summary>
    /// Сохранить или обновить проверку решения по критериям
    /// </summary>
    [Authorize]
    [HttpPut("{submissionId:guid}/assessment")]
    [ProducesResponseType(typeof(SubmissionAssessmentDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SubmissionAssessmentDto>> SaveAssessment(
        Guid submissionId,
        [FromBody] SaveSubmissionAssessmentRequest request)
    {
        var result = await service.SaveAssessmentAsync(submissionId, request);

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

    /// <summary>
    /// Получить свои оценки на курсе
    /// </summary>
    [Authorize]
    [HttpGet("/api/courses/{courseId:guid}/my-grades")]
    [ProducesResponseType(typeof(List<MyCourseGradeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MyCourseGradeDto>>> GetMyCourseGrades(Guid courseId)
    {
        var result = await service.GetMyCourseGradesAsync(courseId);

        return Ok(result);
    }

    /// <summary>
    /// Получить итоговые командные оценки по заданию
    /// </summary>
    [Authorize]
    [HttpGet("/api/assignments/{assignmentId:guid}/team-grades")]
    [ProducesResponseType(typeof(List<AssignmentTeamGradeDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AssignmentTeamGradeDto>>> GetAssignmentTeamGrades(Guid assignmentId)
    {
        var result = await service.GetAssignmentTeamGradesAsync(assignmentId);
        return Ok(result);
    }

    /// <summary>
    /// Получить итоговую оценку своей команды по заданию
    /// </summary>
    [Authorize]
    [HttpGet("/api/assignments/{assignmentId:guid}/my-team-grade")]
    [ProducesResponseType(typeof(AssignmentTeamGradeDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AssignmentTeamGradeDto>> GetMyTeamGrade(Guid assignmentId)
    {
        var result = await service.GetMyTeamGradeAsync(assignmentId);
        return Ok(result);
    }
}
