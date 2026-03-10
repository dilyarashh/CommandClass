using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotteryClass.Data.DTOs;
using PotteryClass.Services;

namespace PotteryClass.Controllers;

[ApiController]
[Route("api/assignments")]
public class CommentsController(ICommentService service) : ControllerBase
{
    /// <summary>
    /// Создать комментарий к заданию
    /// </summary>
    [Authorize]
    [HttpPost("{assignmentId:guid}/comments")]
    [ProducesResponseType(typeof(CommentDto), 200)]
    public async Task<ActionResult<CommentDto>> CreateComment(Guid assignmentId, [FromBody] CreateCommentRequest request)
    {
        var result = await service.CreateCommentAsync(assignmentId, request);

        return Ok(result);
    }

    /// <summary>
    /// Получить список комментариев задания
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<CommentDto>), 200)]
    public async Task<ActionResult<List<CommentDto>>> GetComments(Guid assignmentId)
    {
        var result = await service.GetCommentsAsync(assignmentId);

        return Ok(result);
    }
}