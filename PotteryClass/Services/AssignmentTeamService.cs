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
    ISubmissionRepository submissionRepository,
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

    private static void EnsureRandomDistributionMode(Assignment assignment)
    {
        if (assignment.TeamFormationMode != AssignmentTeamFormationMode.RandomDistribution)
            throw new BadRequestException("Случайное распределение недоступно для этого задания");
    }

    private static void EnsureTeacherManagedMode(Assignment assignment)
    {
        if (assignment.TeamFormationMode != AssignmentTeamFormationMode.TeacherManaged)
            throw new BadRequestException("Ручное распределение недоступно для этого задания");
    }

    private static void EnsureCaptainDraftMode(Assignment assignment)
    {
        if (assignment.TeamFormationMode != AssignmentTeamFormationMode.CaptainDraft)
            throw new BadRequestException("Драфт капитанов недоступен для этого задания");
    }

    private static void EnsureCaptainCanManageTeam(Assignment assignment)
    {
        if (!assignment.RequiresSubmission)
            throw new BadRequestException("Для задания не требуется выбор финального решения");

        if (!IsTeamCompositionLocked(assignment))
            throw new BadRequestException("Состав команды еще не зафиксирован");

        if (assignment.Deadline.HasValue && DateTime.UtcNow > assignment.Deadline.Value)
            throw new BadRequestException("Дедлайн задания уже прошел");
    }

    private static void EnsureTeamFormationIsOpen(Assignment assignment)
    {
        var now = DateTime.UtcNow;

        if (assignment.StartsAtUtc.HasValue && now < assignment.StartsAtUtc.Value)
            throw new BadRequestException("Формирование команд еще не началось");

        if (assignment.TeamFormationEndsAtUtc.HasValue && now > assignment.TeamFormationEndsAtUtc.Value)
            throw new BadRequestException("Этап формирования команд уже завершен");
    }

    private static bool IsTeamCompositionLocked(Assignment assignment)
    {
        if (assignment.TeamCompositionLockedAtUtc.HasValue)
            return true;

        return assignment.StartsAtUtc.HasValue && DateTime.UtcNow >= assignment.StartsAtUtc.Value;
    }

    private static void EnsureTeamCompositionUnlocked(Assignment assignment)
    {
        if (IsTeamCompositionLocked(assignment))
            throw new BadRequestException("Состав команд уже зафиксирован");
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

    private static AssignmentManualDistributionDto MapManualDistribution(
        List<AssignmentTeam> teams,
        List<User> availableStudents)
    {
        return new AssignmentManualDistributionDto
        {
            Teams = teams.Select(Map).ToList(),
            AvailableStudents = availableStudents.Select(x => new CourseStudentDto
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Email = x.Email,
                IsBlocked = false
            }).ToList()
        };
    }

    private static AssignmentDraftStateDto MapDraftState(
        Assignment assignment,
        List<AssignmentTeam> teams,
        List<User> availableStudents)
    {
        return new AssignmentDraftStateDto
        {
            IsStarted = assignment.DraftStartedAtUtc.HasValue,
            IsCompleted = assignment.DraftCompletedAtUtc.HasValue,
            CurrentCaptainUserId = assignment.DraftCurrentCaptainUserId,
            StartedAtUtc = assignment.DraftStartedAtUtc,
            CompletedAtUtc = assignment.DraftCompletedAtUtc,
            Teams = teams.Select(Map).ToList(),
            AvailableStudents = availableStudents.Select(x => new CourseStudentDto
            {
                Id = x.Id,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Email = x.Email,
                IsBlocked = false
            }).ToList()
        };
    }

    private static CaptainTeamDto MapCaptainTeam(AssignmentTeam team, List<Submission> submissions)
    {
        return new CaptainTeamDto
        {
            Id = team.Id,
            AssignmentId = team.AssignmentId,
            Name = team.Name,
            Captain = new AssignmentCaptainDto
            {
                UserId = team.CaptainUserId!.Value,
                FirstName = team.CaptainUser!.FirstName,
                LastName = team.CaptainUser.LastName,
                Email = team.CaptainUser.Email,
                CreatedAtUtc = team.CreatedAtUtc
            },
            FinalSubmissionId = team.FinalSubmissionId,
            Members = team.Members
                .OrderBy(x => x.User.LastName)
                .ThenBy(x => x.User.FirstName)
                .Select(member => new CaptainTeamMemberSubmissionsDto
                {
                    UserId = member.UserId,
                    FirstName = member.User.FirstName,
                    LastName = member.User.LastName,
                    MiddleName = member.User.MiddleName,
                    Submissions = submissions
                        .Where(x => x.StudentId == member.UserId)
                        .Select(MapSubmission)
                        .ToList()
                })
                .ToList()
        };
    }

    private async Task<List<User>> GetAvailableStudentsAsync(Assignment assignment, List<AssignmentTeam> teams)
    {
        var assignedStudentIds = teams
            .SelectMany(x => x.Members)
            .Select(x => x.UserId)
            .ToHashSet();

        var availableStudents = await studentRepository.GetActiveStudentsAsync(assignment.CourseId);
        return availableStudents
            .Where(x => !assignedStudentIds.Contains(x.Id))
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ToList();
    }

    private static Guid? ResolveNextDraftCaptainUserId(
        Assignment assignment,
        List<AssignmentCaptain> captains,
        List<AssignmentTeam> teams,
        bool hasAvailableStudents)
    {
        if (!hasAvailableStudents || captains.Count == 0)
            return null;

        var orderedCaptainIds = captains.Select(x => x.UserId).ToList();
        var currentIndex = assignment.DraftCurrentCaptainUserId.HasValue
            ? orderedCaptainIds.IndexOf(assignment.DraftCurrentCaptainUserId.Value)
            : -1;

        for (var step = 1; step <= orderedCaptainIds.Count; step++)
        {
            var nextIndex = (currentIndex + step) % orderedCaptainIds.Count;
            var nextCaptainId = orderedCaptainIds[nextIndex];
            var team = teams.FirstOrDefault(x => x.CaptainUserId == nextCaptainId);
            if (team == null)
                continue;

            if (!assignment.MaxTeamSize.HasValue || team.Members.Count < assignment.MaxTeamSize.Value)
                return nextCaptainId;
        }

        return null;
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
        EnsureTeamCompositionUnlocked(team.Assignment);
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
        EnsureTeamCompositionUnlocked(team.Assignment);

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

    public async Task<List<AssignmentTeamDto>> DistributeRandomlyAsync(Guid assignmentId)
    {
        var assignment = await assignmentRepository.GetByIdAsync(assignmentId)
            ?? throw new NotFoundException("Задание не найдено");

        await EnsureTeacherOrAdmin(assignment.CourseId);
        EnsureRandomDistributionMode(assignment);
        EnsureTeamFormationIsOpen(assignment);
        EnsureTeamCompositionUnlocked(assignment);
        await EnsureCaptainTeamsCreatedAsync(assignment);

        var teams = await assignmentTeamRepository.GetByAssignmentAsync(assignmentId);
        if (teams.Count == 0)
            throw new BadRequestException("Невозможно распределить студентов без выбранных капитанов");

        var allStudentIds = await studentRepository.GetActiveStudentIdsAsync(assignment.CourseId);
        var existingMemberIds = teams
            .SelectMany(x => x.Members)
            .Select(x => x.UserId)
            .ToHashSet();

        var pendingStudentIds = allStudentIds
            .Where(x => !existingMemberIds.Contains(x))
            .OrderBy(_ => Random.Shared.Next())
            .ToList();

        foreach (var studentId in pendingStudentIds)
        {
            var availableTeams = teams
                .Where(x => !assignment.MaxTeamSize.HasValue || x.Members.Count < assignment.MaxTeamSize.Value)
                .OrderBy(x => x.Members.Count)
                .ThenBy(_ => Random.Shared.Next())
                .ToList();

            if (availableTeams.Count == 0)
                throw new BadRequestException("Недостаточно мест в командах для случайного распределения");

            var team = availableTeams.First();
            team.Members.Add(new AssignmentTeamMember
            {
                TeamId = team.Id,
                UserId = studentId,
                CreatedAtUtc = DateTime.UtcNow
            });
        }

        await assignmentTeamRepository.SaveChangesAsync();
        return teams.Select(Map).ToList();
    }

    public async Task<AssignmentManualDistributionDto> GetManualDistributionAsync(Guid assignmentId)
    {
        var assignment = await assignmentRepository.GetByIdAsync(assignmentId)
            ?? throw new NotFoundException("Задание не найдено");

        await EnsureTeacherOrAdmin(assignment.CourseId);
        EnsureTeacherManagedMode(assignment);
        await EnsureCaptainTeamsCreatedAsync(assignment);

        var teams = await assignmentTeamRepository.GetByAssignmentAsync(assignmentId);
        var assignedStudentIds = teams
            .SelectMany(x => x.Members)
            .Select(x => x.UserId)
            .ToHashSet();

        var availableStudents = await studentRepository.GetActiveStudentsAsync(assignment.CourseId);
        availableStudents = availableStudents
            .Where(x => !assignedStudentIds.Contains(x.Id))
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ToList();

        return MapManualDistribution(teams, availableStudents);
    }

    public async Task<AssignmentDraftStateDto> GetDraftStateAsync(Guid assignmentId)
    {
        var assignment = await assignmentRepository.GetByIdAsync(assignmentId)
            ?? throw new NotFoundException("Задание не найдено");

        await EnsureCourseMemberOrTeacher(assignment.CourseId);
        EnsureCaptainDraftMode(assignment);
        await EnsureCaptainTeamsCreatedAsync(assignment);

        var teams = await assignmentTeamRepository.GetByAssignmentAsync(assignmentId);
        var availableStudents = await GetAvailableStudentsAsync(assignment, teams);
        return MapDraftState(assignment, teams, availableStudents);
    }

    public async Task<CaptainTeamDto> GetCaptainTeamAsync(Guid assignmentId)
    {
        if (currentUser.GetRole() != UserRole.Student)
            throw new ForbiddenException("Нет доступа");

        var currentUserId = currentUser.GetUserId();
        var team = await assignmentTeamRepository.GetCaptainTeamAsync(assignmentId, currentUserId)
            ?? throw new ForbiddenException("Пользователь не является капитаном команды этого задания");

        var memberIds = team.Members.Select(x => x.UserId).ToList();
        var submissions = await submissionRepository.GetByAssignmentAndStudentsAsync(assignmentId, memberIds);
        return MapCaptainTeam(team, submissions);
    }

    public async Task<AssignmentDraftStateDto> StartDraftAsync(Guid assignmentId)
    {
        var assignment = await assignmentRepository.GetByIdAsync(assignmentId)
            ?? throw new NotFoundException("Задание не найдено");

        await EnsureTeacherOrAdmin(assignment.CourseId);
        EnsureCaptainDraftMode(assignment);
        EnsureTeamFormationIsOpen(assignment);
        EnsureTeamCompositionUnlocked(assignment);
        await EnsureCaptainTeamsCreatedAsync(assignment);

        if (assignment.DraftStartedAtUtc.HasValue && !assignment.DraftCompletedAtUtc.HasValue)
            throw new BadRequestException("Драфт уже запущен");

        var captains = await assignmentCaptainRepository.GetByAssignmentAsync(assignmentId);
        if (captains.Count == 0)
            throw new BadRequestException("Невозможно начать драфт без выбранных капитанов");

        var teams = await assignmentTeamRepository.GetByAssignmentAsync(assignmentId);
        var availableStudents = await GetAvailableStudentsAsync(assignment, teams);
        if (availableStudents.Count == 0)
            throw new BadRequestException("Нет доступных студентов для драфта");

        assignment.DraftStartedAtUtc = DateTime.UtcNow;
        assignment.DraftCompletedAtUtc = null;
        assignment.DraftCurrentCaptainUserId = captains.OrderBy(x => x.CreatedAtUtc).First().UserId;

        await assignmentRepository.UpdateAsync(assignment);

        return MapDraftState(assignment, teams, availableStudents);
    }

    public async Task<AssignmentDraftStateDto> DraftPickAsync(Guid assignmentId, Guid studentId)
    {
        var assignment = await assignmentRepository.GetByIdAsync(assignmentId)
            ?? throw new NotFoundException("Задание не найдено");

        EnsureCaptainDraftMode(assignment);
        EnsureTeamFormationIsOpen(assignment);
        EnsureTeamCompositionUnlocked(assignment);

        if (!assignment.DraftStartedAtUtc.HasValue)
            throw new BadRequestException("Драфт еще не запущен");

        if (assignment.DraftCompletedAtUtc.HasValue || !assignment.DraftCurrentCaptainUserId.HasValue)
            throw new BadRequestException("Драфт уже завершен");

        var currentUserId = currentUser.GetUserId();
        if (currentUser.GetRole() != UserRole.Student || assignment.DraftCurrentCaptainUserId.Value != currentUserId)
            throw new ForbiddenException("Сейчас не ваша очередь выбора");

        var isStudentOnCourse = await studentRepository.IsStudentAsync(assignment.CourseId, studentId);
        if (!isStudentOnCourse)
            throw new BadRequestException("Студент не состоит в курсе");

        var teams = await assignmentTeamRepository.GetByAssignmentAsync(assignmentId);
        var currentTeam = teams.FirstOrDefault(x => x.CaptainUserId == currentUserId)
                          ?? throw new NotFoundException("Команда капитана не найдена");

        if (assignment.MaxTeamSize.HasValue && currentTeam.Members.Count >= assignment.MaxTeamSize.Value)
            throw new BadRequestException("Нельзя превысить максимальный размер команды");

        var isAlreadyInAssignmentTeam = await assignmentTeamRepository.IsStudentInAssignmentTeamsAsync(assignmentId, studentId);
        if (isAlreadyInAssignmentTeam)
            throw new BadRequestException("Студент уже состоит в одной из команд задания");

        currentTeam.Members.Add(new AssignmentTeamMember
        {
            TeamId = currentTeam.Id,
            UserId = studentId,
            CreatedAtUtc = DateTime.UtcNow
        });

        await assignmentTeamRepository.SaveChangesAsync();

        teams = await assignmentTeamRepository.GetByAssignmentAsync(assignmentId);
        var availableStudents = await GetAvailableStudentsAsync(assignment, teams);
        var captains = await assignmentCaptainRepository.GetByAssignmentAsync(assignmentId);
        var nextCaptainUserId = ResolveNextDraftCaptainUserId(assignment, captains, teams, availableStudents.Count > 0);

        assignment.DraftCurrentCaptainUserId = nextCaptainUserId;
        if (!nextCaptainUserId.HasValue)
            assignment.DraftCompletedAtUtc = DateTime.UtcNow;

        await assignmentRepository.UpdateAsync(assignment);

        return MapDraftState(assignment, teams, availableStudents);
    }

    public async Task SelectFinalSubmissionAsync(Guid assignmentId, Guid submissionId)
    {
        if (currentUser.GetRole() != UserRole.Student)
            throw new ForbiddenException("Нет доступа");

        var currentUserId = currentUser.GetUserId();
        var team = await assignmentTeamRepository.GetCaptainTeamAsync(assignmentId, currentUserId)
            ?? throw new ForbiddenException("Пользователь не является капитаном команды этого задания");

        EnsureCaptainCanManageTeam(team.Assignment);

        var submission = await submissionRepository.GetByIdAsync(submissionId)
            ?? throw new NotFoundException("Решение не найдено");

        if (submission.AssignmentId != assignmentId)
            throw new BadRequestException("Решение не относится к этому заданию");

        if (team.Members.All(x => x.UserId != submission.StudentId))
            throw new BadRequestException("Можно выбрать только решение участника своей команды");

        team.FinalSubmissionId = submissionId;
        await assignmentTeamRepository.SaveChangesAsync();
    }

    public async Task LockCompositionAsync(Guid assignmentId)
    {
        var assignment = await assignmentRepository.GetByIdAsync(assignmentId)
            ?? throw new NotFoundException("Задание не найдено");

        await EnsureTeacherOrAdmin(assignment.CourseId);

        if (assignment.TeamCompositionLockedAtUtc.HasValue)
            throw new BadRequestException("Состав команд уже зафиксирован");

        assignment.TeamCompositionLockedAtUtc = DateTime.UtcNow;
        await assignmentRepository.UpdateAsync(assignment);
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
            FinalSubmissionId = team.FinalSubmissionId,
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

    private static SubmissionDto MapSubmission(Submission submission)
    {
        return new SubmissionDto
        {
            Id = submission.Id,
            AssignmentId = submission.AssignmentId,
            StudentId = submission.StudentId,
            FirstName = submission.Student?.FirstName,
            LastName = submission.Student?.LastName,
            MiddleName = submission.Student?.MiddleName,
            Created = submission.Created,
            Grade = submission.Grade,
            TeacherComment = submission.TeacherComment,
            GradedByTeacherId = submission.GradedByTeacherId,
            GradedAtUtc = submission.GradedAtUtc,
            Status = submission.Status,
            Files = submission.Files.Select(f => new SubmissionFileDto
            {
                Id = f.Id,
                FileName = f.FileName,
                Url = f.Url,
                MimeType = f.MimeType,
                Size = f.Size,
                Type = f.Type
            }).ToList()
        };
    }
}
