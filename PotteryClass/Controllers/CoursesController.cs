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
}