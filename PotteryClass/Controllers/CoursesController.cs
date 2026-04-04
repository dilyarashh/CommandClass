using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotteryClass.Data.DTOs;
using PotteryClass.Data.Entities.Enums;
using PotteryClass.Services;

namespace PotteryClass.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController(ICourseService service) : ControllerBase
{
    /// <summary>
    /// Создать курс (Admin или Teacher)
    /// </summary>
    [Authorize(Roles = "Admin,Teacher")]
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
    /// Получить список своих курсов
    /// </summary>
    [HttpGet("my")]
    public async Task<ActionResult<List<MyCourseDto>>> GetMyCourses([FromQuery] MyCoursesFilter filter = MyCoursesFilter.All)
    {
        var result = await service.GetMyCoursesAsync(filter);
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
    /// Отправить курс в архив (только Admin)
    /// </summary>
    [HttpPost("{courseId:guid}/archive")]
    public async Task<IActionResult> ArchiveCourse(Guid courseId)
    {
        await service.ArchiveCourseAsync(courseId);

        return NoContent();
    }

    /// <summary> 
    /// Достать курс из архива (только Admin)
    /// </summary>
    [HttpPost("{courseId:guid}/restore")]
    public async Task<IActionResult> RestoreCourse(Guid courseId)
    {
        await service.RestoreCourseAsync(courseId);

        return NoContent();
    }

    /// <summary>
    /// Получить список всех курсов (только Admin)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<List<CourseDto>>> GetAllCourses()
    {
        var result = await service.GetAllCoursesAsync();
        return Ok(result);
    }

    /// <summary>
    /// Обновить курс (Admin или преподаватель курса)
    /// </summary>
    [Authorize(Roles = "Admin,Teacher")]
    [HttpPut("{courseId:guid}")]
    public async Task<IActionResult> UpdateCourse(Guid courseId, [FromBody] UpdateCourseRequest dto)
    {
        await service.UpdateCourseAsync(courseId, dto);

        return NoContent();
    }

    /// <summary>
    /// Покинуть курс
    /// </summary>
    [Authorize]
    [HttpPost("{courseId:guid}/leave")]
    public async Task<IActionResult> LeaveCourse(Guid courseId)
    {
        await service.LeaveCourseAsync(courseId);

        return NoContent();
    }

}
