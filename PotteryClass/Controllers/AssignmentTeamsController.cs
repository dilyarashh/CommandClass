using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PotteryClass.Data.DTOs;
using PotteryClass.Services;

namespace PotteryClass.Controllers;

[ApiController]
[Route("api/assignments")]
public class AssignmentTeamsController(IAssignmentTeamService service) : ControllerBase
{
    /// <summary>
    /// Получить команды задания
    /// </summary>
    [Authorize]
    [HttpGet("{assignmentId:guid}/teams")]
    public async Task<ActionResult<List<AssignmentTeamDto>>> GetTeams(Guid assignmentId)
    {
        var result = await service.GetByAssignmentAsync(assignmentId);
        return Ok(result);
    }

    /// <summary>
    /// Получить данные для ручного распределения команд
    /// </summary>
    [Authorize]
    [HttpGet("{assignmentId:guid}/teams/manual-distribution")]
    public async Task<ActionResult<AssignmentManualDistributionDto>> GetManualDistribution(Guid assignmentId)
    {
        var result = await service.GetManualDistributionAsync(assignmentId);
        return Ok(result);
    }

    /// <summary>
    /// Получить состояние драфта капитанов
    /// </summary>
    [Authorize]
    [HttpGet("{assignmentId:guid}/teams/draft")]
    public async Task<ActionResult<AssignmentDraftStateDto>> GetDraftState(Guid assignmentId)
    {
        var result = await service.GetDraftStateAsync(assignmentId);
        return Ok(result);
    }

    /// <summary>
    /// Создать команду для задания
    /// </summary>
    [Authorize]
    [HttpPost("{assignmentId:guid}/teams")]
    public async Task<ActionResult<AssignmentTeamDto>> CreateTeam(
        Guid assignmentId,
        [FromBody] CreateAssignmentTeamRequest request)
    {
        var result = await service.CreateAsync(assignmentId, request);
        return Ok(result);
    }

    /// <summary>
    /// Случайно распределить студентов по командам
    /// </summary>
    [Authorize]
    [HttpPost("{assignmentId:guid}/teams/random-distribution")]
    public async Task<ActionResult<List<AssignmentTeamDto>>> DistributeRandomly(Guid assignmentId)
    {
        var result = await service.DistributeRandomlyAsync(assignmentId);
        return Ok(result);
    }

    /// <summary>
    /// Запустить драфт капитанов
    /// </summary>
    [Authorize]
    [HttpPost("{assignmentId:guid}/teams/draft/start")]
    public async Task<ActionResult<AssignmentDraftStateDto>> StartDraft(Guid assignmentId)
    {
        var result = await service.StartDraftAsync(assignmentId);
        return Ok(result);
    }

    /// <summary>
    /// Зафиксировать состав команд
    /// </summary>
    [Authorize]
    [HttpPost("{assignmentId:guid}/teams/lock")]
    public async Task<IActionResult> LockComposition(Guid assignmentId)
    {
        await service.LockCompositionAsync(assignmentId);
        return NoContent();
    }

    /// <summary>
    /// Добавить участника в команду
    /// </summary>
    [Authorize]
    [HttpPost("teams/{teamId:guid}/members/{studentId:guid}")]
    public async Task<IActionResult> AddMember(Guid teamId, Guid studentId)
    {
        await service.AddMemberAsync(teamId, studentId);
        return NoContent();
    }

    /// <summary>
    /// Вступить в команду
    /// </summary>
    [Authorize]
    [HttpPost("teams/{teamId:guid}/join-self")]
    public async Task<IActionResult> JoinSelf(Guid teamId)
    {
        await service.JoinSelfAsync(teamId);
        return NoContent();
    }

    /// <summary>
    /// Выбрать участника в драфте
    /// </summary>
    [Authorize]
    [HttpPost("{assignmentId:guid}/teams/draft/pick/{studentId:guid}")]
    public async Task<ActionResult<AssignmentDraftStateDto>> DraftPick(Guid assignmentId, Guid studentId)
    {
        var result = await service.DraftPickAsync(assignmentId, studentId);
        return Ok(result);
    }

    /// <summary>
    /// Удалить участника из команды
    /// </summary>
    [Authorize]
    [HttpDelete("teams/{teamId:guid}/members/{studentId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid teamId, Guid studentId)
    {
        await service.RemoveMemberAsync(teamId, studentId);
        return NoContent();
    }

    /// <summary>
    /// Выйти из команды
    /// </summary>
    [Authorize]
    [HttpDelete("teams/{teamId:guid}/leave-self")]
    public async Task<IActionResult> LeaveSelf(Guid teamId)
    {
        await service.LeaveSelfAsync(teamId);
        return NoContent();
    }
}
