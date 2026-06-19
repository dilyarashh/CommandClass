using System.Text.Json;
using FluentValidation;
using PotteryClass.Data.DTOs;
using PotteryClass.Data.Entities;
using PotteryClass.Data.Entities.Enums;
using PotteryClass.Data.Repositories;
using PotteryClass.Infrastructure.Auth;
using PotteryClass.Infrastructure.Errors.Exceptions;
using ValidationException = PotteryClass.Infrastructure.Errors.Exceptions.ValidationException;

namespace PotteryClass.Services;

public class AssignmentService(
    IAssignmentRepository assignmentRepository,
    ICourseTeacherRepository teacherRepository,
    ICourseStudentRepository studentRepository,
    IAssignmentTeamRepository assignmentTeamRepository,
    IPeerReviewAssignmentRepository peerReviewAssignmentRepository,
    ICurrentUser currentUser,
    IFileStorageService fileStorage,
    IValidator<AssignmentGradingRulesDto> gradingRulesValidator)
    : IAssignmentService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const decimal DefaultPeerReviewPenaltyPercent = 20m;

    private readonly IAssignmentRepository _assignmentRepository = assignmentRepository;
    private readonly ICourseTeacherRepository _teacherRepository = teacherRepository;
    private readonly ICourseStudentRepository _studentRepository = studentRepository;
    private readonly IAssignmentTeamRepository _assignmentTeamRepository = assignmentTeamRepository;
    private readonly IPeerReviewAssignmentRepository _peerReviewAssignmentRepository = peerReviewAssignmentRepository;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IFileStorageService _fileStorage = fileStorage;
    private readonly IValidator<AssignmentGradingRulesDto> _gradingRulesValidator = gradingRulesValidator;

    private async Task EnsureTeacherOrAdmin(Guid courseId)
    {
        var role = _currentUser.GetRole();

        if (role == UserRole.Admin)
            return;

        var userId = _currentUser.GetUserId();

        var isTeacher = await _teacherRepository.IsTeacherAsync(courseId, userId);

        if (!isTeacher)
            throw new ForbiddenException("РќРµС‚ РґРѕСЃС‚СѓРїР°");
    }

    private async Task EnsureCourseMember(Guid courseId)
    {
        var role = _currentUser.GetRole();

        if (role == UserRole.Admin)
            return;

        var userId = _currentUser.GetUserId();

        var isTeacher = await _teacherRepository.IsTeacherAsync(courseId, userId);

        if (isTeacher)
            return;

        var isStudent = await _studentRepository.IsStudentAsync(courseId, userId);

        if (!isStudent)
            throw new ForbiddenException("Нет доступа");
    }

    private async Task EnsureGradingRulesReadableAsync(Assignment assignment)
    {
        var role = _currentUser.GetRole();

        if (role == UserRole.Admin)
            return;

        var userId = _currentUser.GetUserId();
        var isTeacher = await _teacherRepository.IsTeacherAsync(assignment.CourseId, userId);

        if (isTeacher)
            return;

        var isStudent = await _studentRepository.IsStudentAsync(assignment.CourseId, userId);
        if (!isStudent)
            throw new ForbiddenException("Нет доступа");

        if (!assignment.IsVisible)
            throw new ForbiddenException("Задание скрыто");
    }

    private async Task EnsureAssignmentVisibleToCurrentUser(Assignment assignment)
    {
        var role = _currentUser.GetRole();

        if (role == UserRole.Admin)
            return;

        var userId = _currentUser.GetUserId();
        var isTeacher = await _teacherRepository.IsTeacherAsync(assignment.CourseId, userId);

        if (isTeacher)
            return;

        var isStudent = await _studentRepository.IsStudentAsync(assignment.CourseId, userId);
        if (!isStudent)
            throw new ForbiddenException("РќРµС‚ РґРѕСЃС‚СѓРїР°");

        if (!assignment.IsVisible)
            throw new ForbiddenException("Р—Р°РґР°РЅРёРµ СЃРєСЂС‹С‚Рѕ");

        var availableAtUtc = assignment.TeamFormationEndsAtUtc ?? assignment.StartsAtUtc;
        if (availableAtUtc.HasValue && DateTime.UtcNow < availableAtUtc.Value)
            throw new ForbiddenException("Задание пока недоступно");
    }

    private static async Task ValidateAndThrowAsync<T>(IValidator<T> validator, T dto)
    {
        var validationResult = await validator.ValidateAsync(dto);

        if (validationResult.IsValid)
            return;

        var errors = validationResult.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        throw new ValidationException(errors);
    }

    private static void ValidateAssignmentSchedule(
        DateTime? startsAtUtc,
        DateTime? deadline)
    {
        if (startsAtUtc.HasValue && deadline.HasValue && startsAtUtc > deadline)
            throw new BadRequestException("Р”Р°С‚Р° СЃС‚Р°СЂС‚Р° РґРѕР»Р¶РЅР° Р±С‹С‚СЊ СЂР°РЅСЊС€Рµ РґРµРґР»Р°Р№РЅР°");
    }

    private static void ValidateTeamSize(int? minTeamSize, int? maxTeamSize)
    {
        if (minTeamSize.HasValue && minTeamSize.Value < 1)
            throw new BadRequestException("РњРёРЅРёРјР°Р»СЊРЅС‹Р№ СЂР°Р·РјРµСЂ РєРѕРјР°РЅРґС‹ РґРѕР»Р¶РµРЅ Р±С‹С‚СЊ РЅРµ РјРµРЅСЊС€Рµ 1");

        if (maxTeamSize.HasValue && maxTeamSize.Value < 1)
            throw new BadRequestException("РњР°РєСЃРёРјР°Р»СЊРЅС‹Р№ СЂР°Р·РјРµСЂ РєРѕРјР°РЅРґС‹ РґРѕР»Р¶РµРЅ Р±С‹С‚СЊ РЅРµ РјРµРЅСЊС€Рµ 1");

        if (minTeamSize.HasValue && maxTeamSize.HasValue && minTeamSize > maxTeamSize)
            throw new BadRequestException("РњРёРЅРёРјР°Р»СЊРЅС‹Р№ СЂР°Р·РјРµСЂ РєРѕРјР°РЅРґС‹ РґРѕР»Р¶РµРЅ Р±С‹С‚СЊ РЅРµ Р±РѕР»СЊС€Рµ РјР°РєСЃРёРјР°Р»СЊРЅРѕРіРѕ");
    }

    private static AssignmentTeamFormationMode ParseTeamFormationMode(string? mode)
    {
        if (string.IsNullOrWhiteSpace(mode))
            return AssignmentTeamFormationMode.TeacherManaged;

        return mode.Trim().ToLowerInvariant() switch
        {
            AssignmentTeamFormationModeDto.TeacherManaged => AssignmentTeamFormationMode.TeacherManaged,
            AssignmentTeamFormationModeDto.StudentSelfSelection => AssignmentTeamFormationMode.StudentSelfSelection,
            AssignmentTeamFormationModeDto.RandomDistribution => AssignmentTeamFormationMode.RandomDistribution,
            AssignmentTeamFormationModeDto.CaptainDraft => AssignmentTeamFormationMode.CaptainDraft,
            _ => throw new BadRequestException("РќРµРёР·РІРµСЃС‚РЅС‹Р№ СЂРµР¶РёРј С„РѕСЂРјРёСЂРѕРІР°РЅРёСЏ РєРѕРјР°РЅРґ")
        };
    }

    private static string MapTeamFormationMode(AssignmentTeamFormationMode mode)
    {
        return mode switch
        {
            AssignmentTeamFormationMode.TeacherManaged => AssignmentTeamFormationModeDto.TeacherManaged,
            AssignmentTeamFormationMode.StudentSelfSelection => AssignmentTeamFormationModeDto.StudentSelfSelection,
            AssignmentTeamFormationMode.RandomDistribution => AssignmentTeamFormationModeDto.RandomDistribution,
            AssignmentTeamFormationMode.CaptainDraft => AssignmentTeamFormationModeDto.CaptainDraft,
            _ => AssignmentTeamFormationModeDto.TeacherManaged
        };
    }

    private static DateTime? ResolveTeamFormationStartsAtUtc(
        DateTime? startsAtUtc,
        DateTime? captainSelectionEndsAtUtc)
    {
        return captainSelectionEndsAtUtc ?? startsAtUtc;
    }

    private static void ValidateTeamFormationSchedule(
        DateTime? startsAtUtc,
        DateTime? captainSelectionEndsAtUtc,
        DateTime? teamFormationEndsAtUtc,
        DateTime? deadline)
    {
        var teamFormationStartsAtUtc = ResolveTeamFormationStartsAtUtc(startsAtUtc, captainSelectionEndsAtUtc);

        if (captainSelectionEndsAtUtc.HasValue && teamFormationStartsAtUtc.HasValue &&
            captainSelectionEndsAtUtc.Value > teamFormationStartsAtUtc.Value)
            throw new BadRequestException("Р­С‚Р°Рї РІС‹Р±РѕСЂР° РєР°РїРёС‚Р°РЅРѕРІ РґРѕР»Р¶РµРЅ Р·Р°РІРµСЂС€Р°С‚СЊСЃСЏ РЅРµ РїРѕР·Р¶Рµ СЃС‚Р°СЂС‚Р° С„РѕСЂРјРёСЂРѕРІР°РЅРёСЏ РєРѕРјР°РЅРґ");

        if (teamFormationStartsAtUtc.HasValue && teamFormationEndsAtUtc.HasValue &&
            teamFormationStartsAtUtc.Value > teamFormationEndsAtUtc.Value)
            throw new BadRequestException("Р¤РѕСЂРјРёСЂРѕРІР°РЅРёРµ РєРѕРјР°РЅРґ РґРѕР»Р¶РЅРѕ Р·Р°РІРµСЂС€Р°С‚СЊСЃСЏ РЅРµ СЂР°РЅСЊС€Рµ СЃС‚Р°СЂС‚Р° С„РѕСЂРјРёСЂРѕРІР°РЅРёСЏ");

        if (teamFormationEndsAtUtc.HasValue && deadline.HasValue &&
            teamFormationEndsAtUtc.Value > deadline.Value)
            throw new BadRequestException("Р¤РѕСЂРјРёСЂРѕРІР°РЅРёРµ РєРѕРјР°РЅРґ РґРѕР»Р¶РЅРѕ Р·Р°РІРµСЂС€Р°С‚СЊСЃСЏ РЅРµ РїРѕР·Р¶Рµ РґРµРґР»Р°Р№РЅР° Р·Р°РґР°РЅРёСЏ");
    }

    private static void ValidatePeerReviewSettings(
        bool enabled,
        DateTime? startsAtUtc,
        DateTime? endsAtUtc,
        int? requiredReviewsCount,
        decimal penaltyPercent,
        int teamCount)
    {
        if (!enabled)
        {
            if (penaltyPercent < 0 || penaltyPercent > 100)
                throw new BadRequestException("Процент штрафа peer-review должен быть от 0 до 100");

            return;
        }

        if (!startsAtUtc.HasValue)
            throw new BadRequestException("Дата начала peer-review обязательна");

        if (!endsAtUtc.HasValue)
            throw new BadRequestException("Дата окончания peer-review обязательна");

        if (startsAtUtc.Value >= endsAtUtc.Value)
            throw new BadRequestException("Дата начала peer-review должна быть раньше даты окончания");

        if (!requiredReviewsCount.HasValue || requiredReviewsCount.Value < 1)
            throw new BadRequestException("Количество обязательных peer-review проверок должно быть больше 0");

        if (penaltyPercent < 0 || penaltyPercent > 100)
            throw new BadRequestException("Процент штрафа peer-review должен быть от 0 до 100");

        if (teamCount > 0 && requiredReviewsCount.Value > teamCount - 1)
            throw new BadRequestException("Количество обязательных peer-review проверок не может быть больше количества других команд");
    }

    private static PeerReviewAssignmentDto MapPeerReviewAssignment(PeerReviewAssignment assignment)
    {
        return new PeerReviewAssignmentDto
        {
            Id = assignment.Id,
            AssignmentId = assignment.AssignmentId,
            ReviewerTeamId = assignment.ReviewerTeamId,
            ReviewerTeamName = assignment.ReviewerTeam.Name,
            ReviewedTeamId = assignment.ReviewedTeamId,
            ReviewedTeamName = assignment.ReviewedTeam.Name,
            CreatedAtUtc = assignment.CreatedAtUtc
        };
    }

    private static PeerReviewAssignmentResultDto MapPeerReviewAssignmentResult(
        Guid assignmentId,
        int teamsCount,
        int requiredReviewsCount,
        List<PeerReviewAssignment> assignments)
    {
        return new PeerReviewAssignmentResultDto
        {
            AssignmentId = assignmentId,
            TeamsCount = teamsCount,
            RequiredReviewsCount = requiredReviewsCount,
            Assignments = assignments.Select(MapPeerReviewAssignment).ToList()
        };
    }

    private static List<PeerReviewAssignment> BuildPeerReviewAssignments(
        Guid assignmentId,
        IReadOnlyList<AssignmentTeam> teams,
        int requiredReviewsCount)
    {
        var result = new List<PeerReviewAssignment>();
        var now = DateTime.UtcNow;

        for (var reviewerIndex = 0; reviewerIndex < teams.Count; reviewerIndex++)
        {
            var reviewerTeam = teams[reviewerIndex];

            for (var offset = 1; offset <= requiredReviewsCount; offset++)
            {
                var reviewedTeam = teams[(reviewerIndex + offset) % teams.Count];

                result.Add(new PeerReviewAssignment
                {
                    Id = Guid.NewGuid(),
                    AssignmentId = assignmentId,
                    ReviewerTeamId = reviewerTeam.Id,
                    ReviewedTeamId = reviewedTeam.Id,
                    CreatedAtUtc = now
                });
            }
        }

        return result;
    }

    private static bool IsTeamCompositionLocked(Assignment assignment)
    {
        if (assignment.TeamCompositionLockedAtUtc.HasValue)
            return true;

        return assignment.TeamFormationEndsAtUtc.HasValue && DateTime.UtcNow >= assignment.TeamFormationEndsAtUtc.Value;
    }

    private static bool IsClosed(Assignment assignment)
    {
        return assignment.TeamFormationEndsAtUtc.HasValue && DateTime.UtcNow >= assignment.TeamFormationEndsAtUtc.Value;
    }

    private static string ResolveStatus(Assignment assignment)
    {
        var now = DateTime.UtcNow;

        if (assignment.Deadline.HasValue && now > assignment.Deadline.Value)
            return AssignmentStatus.Finished;

        var availableAtUtc = assignment.TeamFormationEndsAtUtc ?? assignment.StartsAtUtc;
        if (availableAtUtc.HasValue && now < availableAtUtc.Value)
            return AssignmentStatus.Hidden;

        return AssignmentStatus.Available;
    }

    private static AssignmentGradingRulesDto CreateDefaultGradingRules()
    {
        return new AssignmentGradingRulesDto
        {
            Mode = AssignmentGradingModeDto.SumPoints,
            MainCriteriaThreshold = new MainCriteriaThresholdSettingsDto
            {
                Enabled = false
            },
            Penalties = new AssignmentPenaltySettingsDto
            {
                Deadline = new PenaltyRuleDto { Enabled = false },
                Progress = new PenaltyRuleDto { Enabled = false },
                RequiredCriteria = new PenaltyRuleDto { Enabled = false }
            }
        };
    }

    private static AssignmentGradingRulesDto DeserializeGradingRules(string? gradingRules)
    {
        if (string.IsNullOrWhiteSpace(gradingRules))
            return CreateDefaultGradingRules();

        try
        {
            var dto = JsonSerializer.Deserialize<AssignmentGradingRulesDto>(gradingRules, JsonOptions);
            return dto ?? CreateDefaultGradingRules();
        }
        catch (JsonException)
        {
            throw new BadRequestException("Invalid assignment grading rules");
        }
    }

    private static string NormalizeGradingMode(string mode)
        => mode.Trim().ToLowerInvariant();

    private static string? NormalizeThresholdBehavior(string? behavior)
        => string.IsNullOrWhiteSpace(behavior)
            ? null
            : behavior.Trim().ToLowerInvariant();

    private static void ValidatePenaltyRule(PenaltyRuleDto dto, string errorMessage)
    {
        if (!dto.Enabled)
        {
            if (dto.Percentage.HasValue)
                throw new BadRequestException(errorMessage);

            return;
        }

        if (!dto.Percentage.HasValue)
            throw new BadRequestException(errorMessage);
    }

    private static void ValidateGradingRulesConsistency(AssignmentGradingRulesDto dto)
    {
        dto.Mode = NormalizeGradingMode(dto.Mode);
        dto.MainCriteriaThreshold.Behavior = NormalizeThresholdBehavior(dto.MainCriteriaThreshold.Behavior);

        if (dto.Mode == AssignmentGradingModeDto.SumPoints && dto.BaseGrade.HasValue)
            throw new BadRequestException("Base grade is not supported for sum_points mode");

        if (dto.Mode == AssignmentGradingModeDto.BaseWithMultipliers && !dto.BaseGrade.HasValue)
            throw new BadRequestException("Base grade is required for base_with_multipliers mode");

        if (!dto.MainCriteriaThreshold.Enabled)
        {
            if (dto.MainCriteriaThreshold.Threshold.HasValue || dto.MainCriteriaThreshold.Behavior is not null)
                throw new BadRequestException("Main criteria threshold settings conflict");
        }
        else
        {
            if (!dto.MainCriteriaThreshold.Threshold.HasValue)
                throw new BadRequestException("Main criteria threshold is required");

            if (string.IsNullOrWhiteSpace(dto.MainCriteriaThreshold.Behavior))
                throw new BadRequestException("Main criteria threshold behavior is required");
        }

        ValidatePenaltyRule(dto.Penalties.Deadline, "Deadline penalty settings conflict");
        ValidatePenaltyRule(dto.Penalties.Progress, "Progress penalty settings conflict");
        ValidatePenaltyRule(dto.Penalties.RequiredCriteria, "Required criteria penalty settings conflict");
    }

    private async Task<Assignment> GetAssignmentForTeacherAsync(Guid assignmentId)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId)
            ?? throw new NotFoundException("Р—Р°РґР°РЅРёРµ РЅРµ РЅР°Р№РґРµРЅРѕ");

        await EnsureTeacherOrAdmin(assignment.CourseId);
        return assignment;
    }

    public async Task<AssignmentDto> CreateAsync(CreateAssignmentRequest dto)
    {
        var userId = _currentUser.GetUserId();

        await EnsureTeacherOrAdmin(dto.CourseId);
        var teamFormationMode = ParseTeamFormationMode(dto.TeamFormationMode);
        ValidateAssignmentSchedule(dto.StartsAtUtc, dto.Deadline);
        ValidateTeamSize(dto.MinTeamSize, dto.MaxTeamSize);
        ValidateTeamFormationSchedule(dto.StartsAtUtc, dto.CaptainSelectionEndsAtUtc, dto.TeamFormationEndsAtUtc, dto.Deadline);
        var peerReviewPenaltyPercent = dto.PeerReviewPenaltyPercent ?? DefaultPeerReviewPenaltyPercent;
        ValidatePeerReviewSettings(
            dto.PeerReviewEnabled,
            dto.PeerReviewStartsAtUtc,
            dto.PeerReviewEndsAtUtc,
            dto.PeerReviewRequiredReviewsCount,
            peerReviewPenaltyPercent,
            teamCount: 0);

        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            CourseId = dto.CourseId,
            CreatedById = userId,
            Title = dto.Title.Trim(),
            Text = dto.Text.Trim(),
            StartsAtUtc = dto.StartsAtUtc,
            MinTeamSize = dto.MinTeamSize,
            MaxTeamSize = dto.MaxTeamSize,
            TeamFormationMode = teamFormationMode,
            CaptainSelectionEndsAtUtc = dto.CaptainSelectionEndsAtUtc,
            TeamFormationEndsAtUtc = dto.TeamFormationEndsAtUtc,
            RequiresSubmission = dto.RequiresSubmission,
            IsVisible = dto.IsVisible,
            Deadline = dto.Deadline,
            PeerReviewEnabled = dto.PeerReviewEnabled,
            PeerReviewStartsAtUtc = dto.PeerReviewStartsAtUtc,
            PeerReviewEndsAtUtc = dto.PeerReviewEndsAtUtc,
            PeerReviewRequiredReviewsCount = dto.PeerReviewRequiredReviewsCount,
            PeerReviewPenaltyPercent = peerReviewPenaltyPercent,
            GradingRules = JsonSerializer.Serialize(CreateDefaultGradingRules(), JsonOptions),
            Created = DateTime.UtcNow
        };

        await _assignmentRepository.AddAsync(assignment);

        return Map(assignment);
    }

    public async Task<AssignmentDto> GetByIdAsync(Guid id)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Р—Р°РґР°РЅРёРµ РЅРµ РЅР°Р№РґРµРЅРѕ");

        await EnsureAssignmentVisibleToCurrentUser(assignment);

        return MapAssignment(assignment);
    }

    public async Task<AssignmentDto> UpdateAsync(Guid id, UpdateAssignmentRequest dto)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Р—Р°РґР°РЅРёРµ РЅРµ РЅР°Р№РґРµРЅРѕ");

        await EnsureTeacherOrAdmin(assignment.CourseId);

        var nextStartsAtUtc = dto.StartsAtUtc ?? assignment.StartsAtUtc;
        var nextDeadline = dto.Deadline ?? assignment.Deadline;
        var nextMinTeamSize = dto.MinTeamSize ?? assignment.MinTeamSize;
        var nextMaxTeamSize = dto.MaxTeamSize ?? assignment.MaxTeamSize;
        var nextCaptainSelectionEndsAtUtc = dto.CaptainSelectionEndsAtUtc ?? assignment.CaptainSelectionEndsAtUtc;
        var nextTeamFormationEndsAtUtc = dto.TeamFormationEndsAtUtc ?? assignment.TeamFormationEndsAtUtc;
        var nextTeamFormationMode = dto.TeamFormationMode is null
            ? assignment.TeamFormationMode
            : ParseTeamFormationMode(dto.TeamFormationMode);
        var nextPeerReviewEnabled = dto.PeerReviewEnabled ?? assignment.PeerReviewEnabled;
        var nextPeerReviewStartsAtUtc = dto.PeerReviewStartsAtUtc ?? assignment.PeerReviewStartsAtUtc;
        var nextPeerReviewEndsAtUtc = dto.PeerReviewEndsAtUtc ?? assignment.PeerReviewEndsAtUtc;
        var nextPeerReviewRequiredReviewsCount = dto.PeerReviewRequiredReviewsCount ?? assignment.PeerReviewRequiredReviewsCount;
        var nextPeerReviewPenaltyPercent = dto.PeerReviewPenaltyPercent ?? assignment.PeerReviewPenaltyPercent;

        ValidateAssignmentSchedule(nextStartsAtUtc, nextDeadline);
        ValidateTeamSize(nextMinTeamSize, nextMaxTeamSize);
        ValidateTeamFormationSchedule(nextStartsAtUtc, nextCaptainSelectionEndsAtUtc, nextTeamFormationEndsAtUtc, nextDeadline);
        ValidatePeerReviewSettings(
            nextPeerReviewEnabled,
            nextPeerReviewStartsAtUtc,
            nextPeerReviewEndsAtUtc,
            nextPeerReviewRequiredReviewsCount,
            nextPeerReviewPenaltyPercent,
            await _assignmentRepository.CountTeamsAsync(id));

        if (dto.Title is not null)
            assignment.Title = dto.Title.Trim();

        if (dto.Text is not null)
            assignment.Text = dto.Text.Trim();

        if (dto.StartsAtUtc.HasValue)
            assignment.StartsAtUtc = dto.StartsAtUtc;

        if (dto.MinTeamSize.HasValue)
            assignment.MinTeamSize = dto.MinTeamSize;

        if (dto.MaxTeamSize.HasValue)
            assignment.MaxTeamSize = dto.MaxTeamSize;

        if (dto.TeamFormationMode is not null)
            assignment.TeamFormationMode = nextTeamFormationMode;

        if (dto.CaptainSelectionEndsAtUtc.HasValue)
            assignment.CaptainSelectionEndsAtUtc = dto.CaptainSelectionEndsAtUtc;

        if (dto.TeamFormationEndsAtUtc.HasValue)
            assignment.TeamFormationEndsAtUtc = dto.TeamFormationEndsAtUtc;

        if (dto.IsVisible.HasValue)
            assignment.IsVisible = dto.IsVisible.Value;

        if (dto.RequiresSubmission.HasValue)
            assignment.RequiresSubmission = dto.RequiresSubmission.Value;

        if (dto.Deadline.HasValue)
            assignment.Deadline = dto.Deadline;

        if (dto.PeerReviewEnabled.HasValue)
            assignment.PeerReviewEnabled = dto.PeerReviewEnabled.Value;

        if (dto.PeerReviewStartsAtUtc.HasValue)
            assignment.PeerReviewStartsAtUtc = dto.PeerReviewStartsAtUtc;

        if (dto.PeerReviewEndsAtUtc.HasValue)
            assignment.PeerReviewEndsAtUtc = dto.PeerReviewEndsAtUtc;

        if (dto.PeerReviewRequiredReviewsCount.HasValue)
            assignment.PeerReviewRequiredReviewsCount = dto.PeerReviewRequiredReviewsCount;

        if (dto.PeerReviewPenaltyPercent.HasValue)
            assignment.PeerReviewPenaltyPercent = dto.PeerReviewPenaltyPercent.Value;

        await _assignmentRepository.UpdateAsync(assignment);

        return Map(assignment);
    }

    public async Task DeleteAsync(Guid id)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Р—Р°РґР°РЅРёРµ РЅРµ РЅР°Р№РґРµРЅРѕ");

        await EnsureTeacherOrAdmin(assignment.CourseId);

        await _assignmentRepository.DeleteAsync(assignment);
    }

    private static AssignmentDto Map(Assignment assignment)
    {
        return new AssignmentDto
        {
            Id = assignment.Id,
            CourseId = assignment.CourseId,
            Title = assignment.Title,
            Text = assignment.Text,
            Status = ResolveStatus(assignment),
            StartsAtUtc = assignment.StartsAtUtc,
            MinTeamSize = assignment.MinTeamSize,
            MaxTeamSize = assignment.MaxTeamSize,
            TeamFormationMode = MapTeamFormationMode(assignment.TeamFormationMode),
            CaptainSelectionEndsAtUtc = assignment.CaptainSelectionEndsAtUtc,
            TeamFormationStartsAtUtc = ResolveTeamFormationStartsAtUtc(assignment.StartsAtUtc, assignment.CaptainSelectionEndsAtUtc),
            TeamFormationEndsAtUtc = assignment.TeamFormationEndsAtUtc,
            DraftCurrentCaptainUserId = assignment.DraftCurrentCaptainUserId,
            DraftStartedAtUtc = assignment.DraftStartedAtUtc,
            DraftCompletedAtUtc = assignment.DraftCompletedAtUtc,
            IsTeamCompositionLocked = IsTeamCompositionLocked(assignment),
            TeamCompositionLockedAtUtc = assignment.TeamCompositionLockedAtUtc,
            IsVisible = assignment.IsVisible,
            IsClosed = IsClosed(assignment),
            RequiresSubmission = assignment.RequiresSubmission,
            Deadline = assignment.Deadline,
            PeerReviewEnabled = assignment.PeerReviewEnabled,
            PeerReviewStartsAtUtc = assignment.PeerReviewStartsAtUtc,
            PeerReviewEndsAtUtc = assignment.PeerReviewEndsAtUtc,
            PeerReviewRequiredReviewsCount = assignment.PeerReviewRequiredReviewsCount,
            PeerReviewPenaltyPercent = assignment.PeerReviewPenaltyPercent,
            Created = assignment.Created
        };
    }

    public async Task<List<AssignmentFileDto>> AddFileAsync(Guid assignmentId, AssignmentFilesFormRequest dto)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId)
                         ?? throw new NotFoundException("Р—Р°РґР°РЅРёРµ РЅРµ РЅР°Р№РґРµРЅРѕ");

        await EnsureTeacherOrAdmin(assignment.CourseId);

        var result = new List<AssignmentFileDto>();

        foreach (var fileDto in dto.Files)
        {
            byte[] content;
            await using (var ms = new MemoryStream())
            {
                await fileDto.File.CopyToAsync(ms);
                content = ms.ToArray();
            }

            var url = await _fileStorage.UploadFileAsync(content, fileDto.File.FileName, fileDto.File.ContentType);

            var assignmentFile = new AssignmentFile
            {
                Id = Guid.NewGuid(),
                AssignmentId = assignmentId,
                FileName = fileDto.File.FileName,
                Url = url,
                MimeType = fileDto.File.ContentType,
                Size = content.LongLength
            };

            await _assignmentRepository.AddFileAsync(assignmentFile);

            result.Add(new AssignmentFileDto
            {
                Id = assignmentFile.Id,
                FileName = assignmentFile.FileName,
                Url = assignmentFile.Url,
                MimeType = assignmentFile.MimeType,
                Size = assignmentFile.Size
            });
        }

        return result;
    }

    public async Task DeleteFileAsync(Guid assignmentId, List<Guid> fileIds)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId)
                         ?? throw new NotFoundException("Р—Р°РґР°РЅРёРµ РЅРµ РЅР°Р№РґРµРЅРѕ");

        await EnsureTeacherOrAdmin(assignment.CourseId);

        var files = assignment.Files
            .Where(f => fileIds.Contains(f.Id))
            .ToList();

        foreach (var file in files)
        {
            await _fileStorage.DeleteFileAsync(file.Url);
            assignment.Files.Remove(file);
        }

        await _assignmentRepository.UpdateAsync(assignment);
    }

    public async Task<PagedAssignmentResult<AssignmentDto>> GetCourseAssignmentsAsync(
        Guid courseId,
        int page,
        int pageSize)
    {
        await EnsureCourseMember(courseId);

        var (items, total) = await _assignmentRepository.GetByCourseAsync(
            courseId,
            page,
            pageSize);

        return new PagedAssignmentResult<AssignmentDto>
        {
            Items = items.Select(MapAssignment).ToList(),
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<PagedAssignmentResult<AssignmentDto>> GetVisibleCourseAssignmentsAsync(
        Guid courseId,
        int page,
        int pageSize)
    {
        await EnsureCourseMember(courseId);

        var (items, total) = await _assignmentRepository.GetByCourseAsync(
            courseId,
            page,
            pageSize);

        var role = _currentUser.GetRole();
        if (role != UserRole.Admin)
        {
            var userId = _currentUser.GetUserId();
            var isTeacher = await _teacherRepository.IsTeacherAsync(courseId, userId);

            if (!isTeacher)
            {
                items = items
                    .Where(x => x.IsVisible)
                    .ToList();
                total = items.Count;
            }
        }

        return new PagedAssignmentResult<AssignmentDto>
        {
            Items = items.Select(MapAssignment).ToList(),
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    private static AssignmentDto MapAssignment(Assignment assignment)
    {
        return new AssignmentDto
        {
            Id = assignment.Id,
            CourseId = assignment.CourseId,
            Title = assignment.Title,
            Text = assignment.Text,
            Status = ResolveStatus(assignment),
            StartsAtUtc = assignment.StartsAtUtc,
            MinTeamSize = assignment.MinTeamSize,
            MaxTeamSize = assignment.MaxTeamSize,
            TeamFormationMode = MapTeamFormationMode(assignment.TeamFormationMode),
            CaptainSelectionEndsAtUtc = assignment.CaptainSelectionEndsAtUtc,
            TeamFormationStartsAtUtc = ResolveTeamFormationStartsAtUtc(assignment.StartsAtUtc, assignment.CaptainSelectionEndsAtUtc),
            TeamFormationEndsAtUtc = assignment.TeamFormationEndsAtUtc,
            DraftCurrentCaptainUserId = assignment.DraftCurrentCaptainUserId,
            DraftStartedAtUtc = assignment.DraftStartedAtUtc,
            DraftCompletedAtUtc = assignment.DraftCompletedAtUtc,
            IsTeamCompositionLocked = IsTeamCompositionLocked(assignment),
            TeamCompositionLockedAtUtc = assignment.TeamCompositionLockedAtUtc,
            IsVisible = assignment.IsVisible,
            IsClosed = IsClosed(assignment),
            RequiresSubmission = assignment.RequiresSubmission,
            Deadline = assignment.Deadline,
            PeerReviewEnabled = assignment.PeerReviewEnabled,
            PeerReviewStartsAtUtc = assignment.PeerReviewStartsAtUtc,
            PeerReviewEndsAtUtc = assignment.PeerReviewEndsAtUtc,
            PeerReviewRequiredReviewsCount = assignment.PeerReviewRequiredReviewsCount,
            PeerReviewPenaltyPercent = assignment.PeerReviewPenaltyPercent,
            Created = assignment.Created,
            Files = assignment.Files.Select(f => new AssignmentFileDto
            {
                Id = f.Id,
                FileName = f.FileName,
                Url = f.Url,
                MimeType = f.MimeType,
                Size = f.Size
            }).ToList()
        };
    }

    public async Task UpdateVisibilityAsync(Guid id, bool isVisible)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Р—Р°РґР°РЅРёРµ РЅРµ РЅР°Р№РґРµРЅРѕ");

        await EnsureTeacherOrAdmin(assignment.CourseId);

        assignment.IsVisible = isVisible;
        await _assignmentRepository.UpdateAsync(assignment);
    }

    public async Task<AssignmentDto> UpdatePeerReviewAsync(Guid id, UpdateAssignmentPeerReviewRequest dto)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Задание не найдено");

        await EnsureTeacherOrAdmin(assignment.CourseId);

        var penaltyPercent = dto.PeerReviewPenaltyPercent ?? assignment.PeerReviewPenaltyPercent;
        ValidatePeerReviewSettings(
            dto.PeerReviewEnabled,
            dto.PeerReviewStartsAtUtc,
            dto.PeerReviewEndsAtUtc,
            dto.PeerReviewRequiredReviewsCount,
            penaltyPercent,
            await _assignmentRepository.CountTeamsAsync(id));

        assignment.PeerReviewEnabled = dto.PeerReviewEnabled;
        assignment.PeerReviewStartsAtUtc = dto.PeerReviewStartsAtUtc;
        assignment.PeerReviewEndsAtUtc = dto.PeerReviewEndsAtUtc;
        assignment.PeerReviewRequiredReviewsCount = dto.PeerReviewRequiredReviewsCount;
        assignment.PeerReviewPenaltyPercent = penaltyPercent;

        await _assignmentRepository.UpdateAsync(assignment);

        return Map(assignment);
    }

    public async Task<PeerReviewAssignmentResultDto> GetPeerReviewAssignmentsAsync(Guid assignmentId)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId)
            ?? throw new NotFoundException("Задание не найдено");

        await EnsureTeacherOrAdmin(assignment.CourseId);

        if (!assignment.PeerReviewEnabled)
            throw new BadRequestException("Peer-review не включен для задания");

        var teamsCount = await _assignmentRepository.CountTeamsAsync(assignmentId);
        var assignments = await _peerReviewAssignmentRepository.GetByAssignmentAsync(assignmentId);

        return MapPeerReviewAssignmentResult(
            assignmentId,
            teamsCount,
            assignment.PeerReviewRequiredReviewsCount ?? 0,
            assignments);
    }

    public async Task<PeerReviewAssignmentResultDto> GeneratePeerReviewAssignmentsAsync(Guid assignmentId)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId)
            ?? throw new NotFoundException("Задание не найдено");

        await EnsureTeacherOrAdmin(assignment.CourseId);

        if (!assignment.PeerReviewEnabled)
            throw new BadRequestException("Peer-review не включен для задания");

        if (!assignment.PeerReviewRequiredReviewsCount.HasValue)
            throw new BadRequestException("Количество обязательных peer-review проверок не задано");

        var teams = await _assignmentTeamRepository.GetByAssignmentAsync(assignmentId);
        if (teams.Count < 2)
            throw new BadRequestException("Для peer-review нужно минимум две команды");

        var requiredReviewsCount = assignment.PeerReviewRequiredReviewsCount.Value;
        if (requiredReviewsCount < 1)
            throw new BadRequestException("Количество обязательных peer-review проверок должно быть больше 0");

        if (requiredReviewsCount > teams.Count - 1)
            throw new BadRequestException("Количество обязательных peer-review проверок не может быть больше количества других команд");

        var assignments = BuildPeerReviewAssignments(assignmentId, teams, requiredReviewsCount);
        await _peerReviewAssignmentRepository.ReplaceForAssignmentAsync(assignmentId, assignments);

        assignments = await _peerReviewAssignmentRepository.GetByAssignmentAsync(assignmentId);
        return MapPeerReviewAssignmentResult(assignmentId, teams.Count, requiredReviewsCount, assignments);
    }

    public async Task<AssignmentGradingRulesDto> GetGradingRulesAsync(Guid assignmentId)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId)
            ?? throw new NotFoundException("Задание не найдено");

        await EnsureGradingRulesReadableAsync(assignment);

        return DeserializeGradingRules(assignment.GradingRules);
    }

    public async Task<AssignmentGradingRulesDto> UpdateGradingRulesAsync(Guid assignmentId, AssignmentGradingRulesDto dto)
    {
        await ValidateAndThrowAsync(_gradingRulesValidator, dto);
        ValidateGradingRulesConsistency(dto);

        var assignment = await GetAssignmentForTeacherAsync(assignmentId);
        assignment.GradingRules = JsonSerializer.Serialize(dto, JsonOptions);

        await _assignmentRepository.UpdateAsync(assignment);

        return DeserializeGradingRules(assignment.GradingRules);
    }
}
