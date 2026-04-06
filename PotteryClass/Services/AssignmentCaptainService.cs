using PotteryClass.Data.DTOs;
using PotteryClass.Data.Entities;
using PotteryClass.Data.Entities.Enums;
using PotteryClass.Data.Repositories;
using PotteryClass.Infrastructure.Auth;
using PotteryClass.Infrastructure.Errors.Exceptions;

namespace PotteryClass.Services;

public class AssignmentCaptainService(
    IAssignmentRepository assignmentRepository,
    IAssignmentCaptainRepository assignmentCaptainRepository,
    IAssignmentTeamRepository assignmentTeamRepository,
    ICourseTeacherRepository teacherRepository,
    ICourseStudentRepository studentRepository,
    ICurrentUser currentUser) : IAssignmentCaptainService
{
    private async Task<Assignment> GetAssignmentAsync(Guid assignmentId)
    {
        return await assignmentRepository.GetByIdAsync(assignmentId)
               ?? throw new NotFoundException("Задание не найдено");
    }

    private async Task EnsureCourseMemberOrTeacherAsync(Assignment assignment)
    {
        var role = currentUser.GetRole();
        if (role == UserRole.Admin)
            return;

        var userId = currentUser.GetUserId();
        var isTeacher = await teacherRepository.IsTeacherAsync(assignment.CourseId, userId);
        if (isTeacher)
            return;

        var isStudent = await studentRepository.IsStudentAsync(assignment.CourseId, userId);
        if (!isStudent)
            throw new ForbiddenException("Нет доступа");
    }

    private static void EnsureStudentSelfSelectionMode(Assignment assignment)
    {
        if (assignment.TeamFormationMode != AssignmentTeamFormationMode.StudentSelfSelection)
            throw new BadRequestException("Самовыбор капитанов недоступен для этого задания");
    }

    private static void EnsureCaptainSelectionIsOpen(Assignment assignment)
    {
        var now = DateTime.UtcNow;

        if (assignment.CaptainSelectionEndsAtUtc.HasValue && now > assignment.CaptainSelectionEndsAtUtc.Value)
            throw new BadRequestException("Этап выбора капитанов уже завершен");

        if (assignment.StartsAtUtc.HasValue && now >= assignment.StartsAtUtc.Value)
            throw new BadRequestException("Этап выбора капитанов уже завершен");
    }

    public async Task<List<AssignmentCaptainDto>> GetByAssignmentAsync(Guid assignmentId)
    {
        var assignment = await GetAssignmentAsync(assignmentId);
        await EnsureCourseMemberOrTeacherAsync(assignment);

        var captains = await assignmentCaptainRepository.GetByAssignmentAsync(assignmentId);
        return captains.Select(Map).ToList();
    }

    public async Task SelfAssignAsync(Guid assignmentId)
    {
        var assignment = await GetAssignmentAsync(assignmentId);
        EnsureStudentSelfSelectionMode(assignment);
        EnsureCaptainSelectionIsOpen(assignment);

        var userId = currentUser.GetUserId();
        if (currentUser.GetRole() != UserRole.Student)
            throw new ForbiddenException("Только студент может выбрать себя капитаном");

        var isStudent = await studentRepository.IsStudentAsync(assignment.CourseId, userId);
        if (!isStudent)
            throw new ForbiddenException("Нет доступа");

        var alreadyCaptain = await assignmentCaptainRepository.ExistsAsync(assignmentId, userId);
        if (alreadyCaptain)
            throw new BadRequestException("Пользователь уже выбран капитаном");

        var isAlreadyInTeam = await assignmentTeamRepository.IsStudentInAssignmentTeamsAsync(assignmentId, userId);
        if (isAlreadyInTeam)
            throw new BadRequestException("Нельзя выбрать капитаном участника уже сформированной команды");

        await assignmentCaptainRepository.AddAsync(new AssignmentCaptain
        {
            AssignmentId = assignmentId,
            UserId = userId,
            CreatedAtUtc = DateTime.UtcNow
        });
        await assignmentCaptainRepository.SaveChangesAsync();
    }

    public async Task WithdrawSelfAsync(Guid assignmentId)
    {
        var assignment = await GetAssignmentAsync(assignmentId);
        EnsureStudentSelfSelectionMode(assignment);
        EnsureCaptainSelectionIsOpen(assignment);

        var userId = currentUser.GetUserId();
        if (currentUser.GetRole() != UserRole.Student)
            throw new ForbiddenException("Только студент может снять себя с роли капитана");

        var captain = await assignmentCaptainRepository.GetAsync(assignmentId, userId)
                      ?? throw new NotFoundException("Пользователь не выбран капитаном");

        await assignmentCaptainRepository.RemoveAsync(captain);
        await assignmentCaptainRepository.SaveChangesAsync();
    }

    private static AssignmentCaptainDto Map(AssignmentCaptain captain)
    {
        return new AssignmentCaptainDto
        {
            UserId = captain.UserId,
            FirstName = captain.User.FirstName,
            LastName = captain.User.LastName,
            Email = captain.User.Email,
            CreatedAtUtc = captain.CreatedAtUtc
        };
    }
}
