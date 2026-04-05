using PotteryClass.Data.DTOs;
using PotteryClass.Data.Entities;
using PotteryClass.Data.Entities.Enums;
using PotteryClass.Data.Repositories;
using PotteryClass.Infrastructure.Auth;
using PotteryClass.Infrastructure.Errors.Exceptions;

namespace PotteryClass.Services;

public class AssignmentTeamService(
    IAssignmentRepository assignmentRepository,
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

        await EnsureTeacherOrAdmin(assignment.CourseId);

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

        var member = team.Members.FirstOrDefault(x => x.UserId == studentId);
        if (member == null)
            throw new NotFoundException("Участник не найден в команде");

        team.Members.Remove(member);
        await assignmentTeamRepository.SaveChangesAsync();
    }

    private static AssignmentTeamDto Map(AssignmentTeam team)
    {
        return new AssignmentTeamDto
        {
            Id = team.Id,
            AssignmentId = team.AssignmentId,
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
