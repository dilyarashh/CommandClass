using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotteryClass.Data.DTOs;
using PotteryClass.Services;

namespace PotteryClass.Controllers;

[ApiController]
[Route("api/courses")]
public class CourseStudentsController(ICourseService service) : ControllerBase
{
    /// <summary>
    /// Получить список студентов курса по айди (только преподаватель)
    /// </summary>
    [HttpGet("{courseId:guid}/students")]
    public async Task<ActionResult<List<CourseStudentDto>>> GetStudents(Guid courseId)
    {
        var result = await service.GetCourseStudentsAsync(courseId);
        return Ok(result);
    }

    /// <summary>
    /// Добавить студента на курс вручную (преподаватель или администратор)
    /// </summary>
    [Authorize(Roles = "Admin,Teacher")]
    [HttpPost("{courseId:guid}/students/{studentId:guid}")]
    public async Task<IActionResult> AddStudent(Guid courseId, Guid studentId)
    {
        await service.AddStudentAsync(courseId, studentId);
        return NoContent();
    }

    /// <summary>
    /// Удалить студента с курса (преподаватель или администратор)
    /// </summary>
    [Authorize(Roles = "Admin,Teacher")]
    [HttpDelete("{courseId:guid}/students/{studentId:guid}")]
    public async Task<IActionResult> RemoveStudent(Guid courseId, Guid studentId)
    {
        await service.RemoveStudentAsync(courseId, studentId);
        return NoContent();
    }

    /// <summary>
    /// Заблокировать студента по его айди и айди курса (только преподаватель)
    /// </summary>
    [HttpPatch("{courseId:guid}/students/{studentId:guid}/block")]
    public async Task<IActionResult> BlockStudent(Guid courseId, Guid studentId)
    {
        await service.BlockStudentAsync(courseId, studentId);
        return NoContent();
    }

    /// <summary>
    /// Разблокировать студента по его айди и айди курса (только преподаватель)
    /// </summary>
    [HttpPatch("{courseId:guid}/students/{studentId:guid}/unblock")]
    public async Task<IActionResult> UnblockStudent(Guid courseId, Guid studentId)
    {
        await service.UnblockStudentAsync(courseId, studentId);
        return NoContent();
    }
}
