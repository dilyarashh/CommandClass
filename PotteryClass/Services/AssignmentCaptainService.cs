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
    IUserRepository userRepository,
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

    private async Task EnsureTeacherOrAdminAsync(Assignment assignment)
    {
        var role = currentUser.GetRole();
        if (role == UserRole.Admin)
            return;

        var userId = currentUser.GetUserId();
        var isTeacher = await teacherRepository.IsTeacherAsync(assignment.CourseId, userId);
        if (!isTeacher)
            throw new ForbiddenException("Нет доступа");
    }

    private static void EnsureStudentCaptainSelectionMode(Assignment assignment)
    {
        if (assignment.TeamFormationMode == AssignmentTeamFormationMode.TeacherManaged)
            throw new BadRequestException("Самовыбор капитанов недоступен для этого задания");
    }

    private static void EnsureTeacherCaptainSelectionMode(Assignment assignment)
    {
        if (assignment.TeamFormationMode != AssignmentTeamFormationMode.TeacherManaged)
            throw new BadRequestException("Ручное назначение капитанов недоступно для этого задания");
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

    public async Task<CaptainAssignmentContextDto> GetMyContextAsync(Guid assignmentId)
    {
        var assignment = await GetAssignmentAsync(assignmentId);
        await EnsureCourseMemberOrTeacherAsync(assignment);

        var userId = currentUser.GetUserId();
        var team = await assignmentTeamRepository.GetCaptainTeamAsync(assignmentId, userId);

        return new CaptainAssignmentContextDto
        {
            AssignmentId = assignmentId,
            IsCaptain = team is not null,
            TeamId = team?.Id,
            FinalSubmissionId = team?.FinalSubmissionId,
            CanSelectFinalSubmission = team is not null && assignment.RequiresSubmission
        };
    }

    public async Task SelfAssignAsync(Guid assignmentId)
    {
        var assignment = await GetAssignmentAsync(assignmentId);
        EnsureStudentCaptainSelectionMode(assignment);
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

    public async Task AssignAsync(Guid assignmentId, Guid studentId)
    {
        var assignment = await GetAssignmentAsync(assignmentId);
        await EnsureTeacherOrAdminAsync(assignment);
        EnsureTeacherCaptainSelectionMode(assignment);
        EnsureCaptainSelectionIsOpen(assignment);

        var user = await userRepository.GetByIdAsync(studentId)
                   ?? throw new NotFoundException("Пользователь не найден");

        if (user.Role != UserRole.Student)
            throw new BadRequestException("Капитаном можно назначить только студента");

        var isStudent = await studentRepository.IsStudentAsync(assignment.CourseId, studentId);
        if (!isStudent)
            throw new BadRequestException("Студент не состоит в курсе");

        var alreadyCaptain = await assignmentCaptainRepository.ExistsAsync(assignmentId, studentId);
        if (alreadyCaptain)
            throw new BadRequestException("Пользователь уже выбран капитаном");

        var isAlreadyInTeam = await assignmentTeamRepository.IsStudentInAssignmentTeamsAsync(assignmentId, studentId);
        if (isAlreadyInTeam)
            throw new BadRequestException("Нельзя назначить капитаном участника уже сформированной команды");

        await assignmentCaptainRepository.AddAsync(new AssignmentCaptain
        {
            AssignmentId = assignmentId,
            UserId = studentId,
            CreatedAtUtc = DateTime.UtcNow
        });
        await assignmentCaptainRepository.SaveChangesAsync();
    }

    public async Task WithdrawSelfAsync(Guid assignmentId)
    {
        var assignment = await GetAssignmentAsync(assignmentId);
        EnsureStudentCaptainSelectionMode(assignment);
        EnsureCaptainSelectionIsOpen(assignment);

        var userId = currentUser.GetUserId();
        if (currentUser.GetRole() != UserRole.Student)
            throw new ForbiddenException("Только студент может снять себя с роли капитана");

        var captain = await assignmentCaptainRepository.GetAsync(assignmentId, userId)
                      ?? throw new NotFoundException("Пользователь не выбран капитаном");

        await assignmentCaptainRepository.RemoveAsync(captain);
        await assignmentCaptainRepository.SaveChangesAsync();
    }

    public async Task RemoveAsync(Guid assignmentId, Guid studentId)
    {
        var assignment = await GetAssignmentAsync(assignmentId);
        await EnsureTeacherOrAdminAsync(assignment);
        EnsureTeacherCaptainSelectionMode(assignment);
        EnsureCaptainSelectionIsOpen(assignment);

        var captain = await assignmentCaptainRepository.GetAsync(assignmentId, studentId)
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
