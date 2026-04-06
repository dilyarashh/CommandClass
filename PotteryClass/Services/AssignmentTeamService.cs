using PotteryClass.Data.DTOs;
using PotteryClass.Data.Entities;
using PotteryClass.Data.Entities.Enums;
using PotteryClass.Data.Repositories;
using PotteryClass.Infrastructure.Auth;
using PotteryClass.Infrastructure.Errors.Exceptions;

namespace PotteryClass.Services;

public class AssignmentTeamService(
    IAssignmentRepository assignmentRepository,
    IAssignmentCaptainRepository assignmentCaptainRepository,
    IAssignmentTeamRepository assignmentTeamRepository,
    ICourseTeacherRepository teacherRepository,
    ICourseStudentRepository studentRepository,
    IUserRepository userRepository,
    ICurrentUser currentUser) : IAssignmentTeamService
{
    private async Task EnsureTeacherOrAdmin(Guid courseId)
    {
        var role = currentUser.GetRole();
        if (role == UserRole.Admin)
            return;

        var userId = currentUser.GetUserId();
        var isTeacher = await teacherRepository.IsTeacherAsync(courseId, userId);
        if (!isTeacher)
            throw new ForbiddenException("Нет доступа");
    }

    private async Task EnsureCourseMemberOrTeacher(Guid courseId)
    {
        var role = currentUser.GetRole();
        if (role == UserRole.Admin)
            return;

        var userId = currentUser.GetUserId();
        var isTeacher = await teacherRepository.IsTeacherAsync(courseId, userId);
        if (isTeacher)
            return;

        var isStudent = await studentRepository.IsStudentAsync(courseId, userId);
        if (!isStudent)
            throw new ForbiddenException("Нет доступа");
    }

    private static void EnsureStudentSelfSelectionMode(Assignment assignment)
    {
        if (assignment.TeamFormationMode != AssignmentTeamFormationMode.StudentSelfSelection)
            throw new BadRequestException("Свободный набор в команды недоступен для этого задания");
    }

    private static void EnsureTeamFormationIsOpen(Assignment assignment)
    {
        var now = DateTime.UtcNow;

        if (assignment.StartsAtUtc.HasValue && now < assignment.StartsAtUtc.Value)
            throw new BadRequestException("Формирование команд еще не началось");

        if (assignment.TeamFormationEndsAtUtc.HasValue && now > assignment.TeamFormationEndsAtUtc.Value)
            throw new BadRequestException("Этап формирования команд уже завершен");
    }

    private static string BuildCaptainTeamName(User user)
    {
        var fullName = $"{user.FirstName} {user.LastName}".Trim();
        if (string.IsNullOrWhiteSpace(fullName))
            fullName = user.Email;

        return $"Команда {fullName}";
    }

    private async Task EnsureCaptainTeamsCreatedAsync(Assignment assignment)
    {
        var captains = await assignmentCaptainRepository.GetByAssignmentAsync(assignment.Id);

        foreach (var captain in captains)
        {
            var hasTeam = await assignmentTeamRepository.HasCaptainTeamAsync(assignment.Id, captain.UserId);
            if (hasTeam)
                continue;

            var teamId = Guid.NewGuid();
            var team = new AssignmentTeam
            {
                Id = teamId,
                AssignmentId = assignment.Id,
                CaptainUserId = captain.UserId,
                Name = BuildCaptainTeamName(captain.User),
                CreatedAtUtc = DateTime.UtcNow
            };

            team.Members.Add(new AssignmentTeamMember
            {
                TeamId = teamId,
                UserId = captain.UserId,
                CreatedAtUtc = DateTime.UtcNow
            });

            await assignmentTeamRepository.AddAsync(team);
        }

        await assignmentTeamRepository.SaveChangesAsync();
    }

    public async Task<AssignmentTeamDto> CreateAsync(Guid assignmentId, CreateAssignmentTeamRequest request)
    {
        var assignment = await assignmentRepository.GetByIdAsync(assignmentId)
            ?? throw new NotFoundException("Задание не найдено");

        await EnsureTeacherOrAdmin(assignment.CourseId);

        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new BadRequestException("Название команды не может быть пустым");

        var team = new AssignmentTeam
        {
            Id = Guid.NewGuid(),
            AssignmentId = assignmentId,
            Name = name,
            CreatedAtUtc = DateTime.UtcNow
        };

        await assignmentTeamRepository.AddAsync(team);
        await assignmentTeamRepository.SaveChangesAsync();

        return Map(team);
    }

    public async Task<List<AssignmentTeamDto>> GetByAssignmentAsync(Guid assignmentId)
    {
        var assignment = await assignmentRepository.GetByIdAsync(assignmentId)
            ?? throw new NotFoundException("Задание не найдено");

        await EnsureCourseMemberOrTeacher(assignment.CourseId);

        if (assignment.TeamFormationMode == AssignmentTeamFormationMode.StudentSelfSelection)
            await EnsureCaptainTeamsCreatedAsync(assignment);

        if (currentUser.GetRole() == UserRole.Student)
            EnsureStudentSelfSelectionMode(assignment);

        var teams = await assignmentTeamRepository.GetByAssignmentAsync(assignmentId);
        return teams.Select(Map).ToList();
    }

    public async Task AddMemberAsync(Guid teamId, Guid studentId)
    {
        var team = await assignmentTeamRepository.GetByIdAsync(teamId)
            ?? throw new NotFoundException("Команда не найдена");

        await EnsureTeacherOrAdmin(team.Assignment.CourseId);

        var user = await userRepository.GetByIdAsync(studentId)
            ?? throw new NotFoundException("Пользователь не найден");

        if (user.Role != UserRole.Student)
            throw new BadRequestException("В команду можно добавить только студента");

        var isStudentOnCourse = await studentRepository.IsStudentAsync(team.Assignment.CourseId, studentId);
        if (!isStudentOnCourse)
            throw new BadRequestException("Студент не состоит в курсе");

        var isAlreadyInAssignmentTeam = await assignmentTeamRepository.IsStudentInAssignmentTeamsAsync(team.AssignmentId, studentId);
        if (isAlreadyInAssignmentTeam)
            throw new BadRequestException("Студент уже состоит в одной из команд задания");

        if (team.Assignment.MaxTeamSize.HasValue && team.Members.Count >= team.Assignment.MaxTeamSize.Value)
            throw new BadRequestException("Нельзя превысить максимальный размер команды");

        team.Members.Add(new AssignmentTeamMember
        {
            TeamId = team.Id,
            UserId = studentId,
            CreatedAtUtc = DateTime.UtcNow
        });

        await assignmentTeamRepository.SaveChangesAsync();
    }

    public async Task RemoveMemberAsync(Guid teamId, Guid studentId)
    {
        var team = await assignmentTeamRepository.GetByIdAsync(teamId)
            ?? throw new NotFoundException("Команда не найдена");

        await EnsureTeacherOrAdmin(team.Assignment.CourseId);

        if (team.CaptainUserId == studentId)
            throw new BadRequestException("Нельзя удалить капитана из его команды");

        var member = team.Members.FirstOrDefault(x => x.UserId == studentId);
        if (member == null)
            throw new NotFoundException("Участник не найден в команде");

        team.Members.Remove(member);
        await assignmentTeamRepository.SaveChangesAsync();
    }

    public async Task JoinSelfAsync(Guid teamId)
    {
        var team = await assignmentTeamRepository.GetByIdAsync(teamId)
            ?? throw new NotFoundException("Команда не найдена");

        EnsureStudentSelfSelectionMode(team.Assignment);
        EnsureTeamFormationIsOpen(team.Assignment);
        await EnsureCaptainTeamsCreatedAsync(team.Assignment);

        var userId = currentUser.GetUserId();
        if (currentUser.GetRole() != UserRole.Student)
            throw new ForbiddenException("Только студент может вступить в команду");

        var isStudentOnCourse = await studentRepository.IsStudentAsync(team.Assignment.CourseId, userId);
        if (!isStudentOnCourse)
            throw new ForbiddenException("Нет доступа");

        var isAlreadyInAssignmentTeam = await assignmentTeamRepository.IsStudentInAssignmentTeamsAsync(team.AssignmentId, userId);
        if (isAlreadyInAssignmentTeam)
            throw new BadRequestException("Студент уже состоит в одной из команд задания");

        if (team.Assignment.MaxTeamSize.HasValue && team.Members.Count >= team.Assignment.MaxTeamSize.Value)
            throw new BadRequestException("Нельзя превысить максимальный размер команды");

        team.Members.Add(new AssignmentTeamMember
        {
            TeamId = team.Id,
            UserId = userId,
            CreatedAtUtc = DateTime.UtcNow
        });

        await assignmentTeamRepository.SaveChangesAsync();
    }

    public async Task LeaveSelfAsync(Guid teamId)
    {
        var team = await assignmentTeamRepository.GetByIdAsync(teamId)
            ?? throw new NotFoundException("Команда не найдена");

        EnsureStudentSelfSelectionMode(team.Assignment);
        EnsureTeamFormationIsOpen(team.Assignment);

        var userId = currentUser.GetUserId();
        if (currentUser.GetRole() != UserRole.Student)
            throw new ForbiddenException("Только студент может выйти из команды");

        if (team.CaptainUserId == userId)
            throw new BadRequestException("Капитан не может выйти из своей команды");

        var member = team.Members.FirstOrDefault(x => x.UserId == userId);
        if (member == null)
            throw new NotFoundException("Пользователь не состоит в команде");

        team.Members.Remove(member);
        await assignmentTeamRepository.SaveChangesAsync();
    }

    private static AssignmentTeamDto Map(AssignmentTeam team)
    {
        return new AssignmentTeamDto
        {
            Id = team.Id,
            AssignmentId = team.AssignmentId,
            Captain = team.CaptainUserId.HasValue && team.CaptainUser is not null
                ? new AssignmentCaptainDto
                {
                    UserId = team.CaptainUserId.Value,
                    FirstName = team.CaptainUser.FirstName,
                    LastName = team.CaptainUser.LastName,
                    Email = team.CaptainUser.Email,
                    CreatedAtUtc = team.CreatedAtUtc
                }
                : null,
            Name = team.Name,
            CreatedAtUtc = team.CreatedAtUtc,
            Members = team.Members.Select(x => new AssignmentTeamMemberDto
            {
                UserId = x.UserId,
                FirstName = x.User.FirstName,
                LastName = x.User.LastName,
                Email = x.User.Email,
                CreatedAtUtc = x.CreatedAtUtc
            }).ToList()
        };
    }
}
