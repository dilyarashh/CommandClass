using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotteryClass.Data.DTOs;
using PotteryClass.Services;

namespace PotteryClass.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController(ICourseService service) : ControllerBase
{
    /// <summary>
    /// Создать курс (только Admin)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(CourseDto), 200)]
    public async Task<ActionResult<CourseDto>> Create([FromBody] CreateCourseRequest dto)
    {
        var course = await service.CreateCourseAsync(dto);
        return Ok(course);
    }

    /// <summary>
    /// Присоединиться к курсу
    /// </summary>
    [Authorize]
    [HttpPost("join")]
    [ProducesResponseType(typeof(CourseDto), 200)]
    public async Task<ActionResult<CourseDto>> Join([FromBody] JoinCourseRequest dto)
    {
        var course = await service.JoinCourseAsync(dto);
        return Ok(course);
    }

    /// <summary>
    /// Получить список всех курсов пользователя
    /// </summary>
    [HttpGet("my")]
    public async Task<ActionResult<List<MyCourseDto>>> GetMyCourses()
    {
        var result = await service.GetMyCoursesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Получить информацию о курсе по айди
    /// </summary>
    [HttpGet("{courseId:guid}")]
    public async Task<ActionResult<CourseDto>> GetCourseById(Guid courseId)
    {
        var result = await service.GetCourseByIdAsync(courseId);

        return Ok(result);
    }

    /// <summary>
    /// Получить список студентов курса по айди (доступно только преподавателю)
    /// </summary>
    [HttpGet("{courseId:guid}/students")]
    public async Task<ActionResult<List<CourseStudentDto>>> GetStudents(Guid courseId)
    {
        var result = await service.GetCourseStudentsAsync(courseId);
        return Ok(result);
    }

    /// <summary>
    /// Заблокировать студента по его айди и айди курса (доступно только преподавателю)
    /// </summary>
    [HttpPatch("{courseId:guid}/students/{studentId:guid}/block")]
    public async Task<IActionResult> BlockStudent(Guid courseId, Guid studentId)
    {
        await service.BlockStudentAsync(courseId, studentId);
        return NoContent();
    }
}