using Microsoft.AspNetCore.Mvc;
using PotteryClass.Data.DTOs;
using PotteryClass.Services;

namespace PotteryClass.Controllers;

[ApiController]
[Route("api/courses")]
public class CourseTeachersController(ICourseService service) : ControllerBase
{
    /// <summary>
    /// Получить список преподавателей курса
    /// </summary>
    [HttpGet("{courseId:guid}/teachers")]
    public async Task<ActionResult<List<CourseTeacherDto>>> GetTeachers(Guid courseId)
    {
        var result = await service.GetCourseTeachersAsync(courseId);
        return Ok(result);
    }

    /// <summary>
    /// Назначить нового преподавателя на курс (только Admin)
    /// </summary>
    [HttpPost("{courseId:guid}/teachers/{teacherId:guid}")]
    public async Task<IActionResult> AddTeacher(Guid courseId, Guid teacherId)
    {
        await service.AddTeacherAsync(courseId, teacherId);
        return NoContent();
    }

    /// <summary>
    /// Снять пользователя с должности преподавателя курса (только Admin)
    /// </summary>
    [HttpDelete("{courseId:guid}/teachers/{teacherId:guid}")]
    public async Task<IActionResult> RemoveTeacher(Guid courseId, Guid teacherId)
    {
        await service.RemoveTeacherAsync(courseId, teacherId);
        return NoContent();
    }
}
