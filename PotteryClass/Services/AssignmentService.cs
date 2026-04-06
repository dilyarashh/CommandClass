using PotteryClass.Data.DTOs;
using PotteryClass.Data.Entities;
using PotteryClass.Data.Entities.Enums;
using PotteryClass.Data.Repositories;
using PotteryClass.Infrastructure.Auth;
using PotteryClass.Infrastructure.Errors.Exceptions;

namespace PotteryClass.Services;

public class AssignmentService(
    IAssignmentRepository assignmentRepository,
    ICourseTeacherRepository teacherRepository,
    ICourseStudentRepository studentRepository,
    ICurrentUser currentUser,
    IFileStorageService fileStorage)
    : IAssignmentService
{
    private readonly IAssignmentRepository _assignmentRepository = assignmentRepository;
    private readonly ICourseTeacherRepository _teacherRepository = teacherRepository;
    private readonly ICourseStudentRepository _studentRepository = studentRepository;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IFileStorageService _fileStorage = fileStorage;


    private async Task EnsureTeacherOrAdmin(Guid courseId)
    {
        var role = _currentUser.GetRole();

        if (role == UserRole.Admin)
            return;

        var userId = _currentUser.GetUserId();

        var isTeacher = await _teacherRepository.IsTeacherAsync(courseId, userId);

        if (!isTeacher)
            throw new ForbiddenException("Нет доступа");
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
            throw new ForbiddenException("Нет доступа");

        if (assignment.StartsAtUtc.HasValue && DateTime.UtcNow < assignment.StartsAtUtc.Value)
            throw new ForbiddenException("Задание пока недоступно");
    }

    private static void ValidateAssignmentSchedule(
        DateTime? startsAtUtc,
        DateTime? deadline)
    {
        if (startsAtUtc.HasValue && deadline.HasValue && startsAtUtc > deadline)
            throw new BadRequestException("Дата старта должна быть раньше дедлайна");
    }

    private static void ValidateTeamSize(int? minTeamSize, int? maxTeamSize)
    {
        if (minTeamSize.HasValue && minTeamSize.Value < 1)
            throw new BadRequestException("Минимальный размер команды должен быть не меньше 1");

        if (maxTeamSize.HasValue && maxTeamSize.Value < 1)
            throw new BadRequestException("Максимальный размер команды должен быть не меньше 1");

        if (minTeamSize.HasValue && maxTeamSize.HasValue && minTeamSize > maxTeamSize)
            throw new BadRequestException("Минимальный размер команды должен быть не больше максимального");
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
            _ => throw new BadRequestException("Неизвестный режим формирования команд")
        };
    }

    private static string MapTeamFormationMode(AssignmentTeamFormationMode mode)
    {
        return mode switch
        {
            AssignmentTeamFormationMode.TeacherManaged => AssignmentTeamFormationModeDto.TeacherManaged,
            AssignmentTeamFormationMode.StudentSelfSelection => AssignmentTeamFormationModeDto.StudentSelfSelection,
            AssignmentTeamFormationMode.RandomDistribution => AssignmentTeamFormationModeDto.RandomDistribution,
            _ => AssignmentTeamFormationModeDto.TeacherManaged
        };
    }

    private static DateTime? ResolveTeamFormationStartsAtUtc(
        DateTime? startsAtUtc,
        DateTime? captainSelectionEndsAtUtc)
    {
        return startsAtUtc ?? captainSelectionEndsAtUtc;
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
            throw new BadRequestException("Этап выбора капитанов должен завершаться не позже старта формирования команд");

        if (teamFormationStartsAtUtc.HasValue && teamFormationEndsAtUtc.HasValue &&
            teamFormationStartsAtUtc.Value > teamFormationEndsAtUtc.Value)
            throw new BadRequestException("Формирование команд должно завершаться не раньше старта формирования");

        if (teamFormationEndsAtUtc.HasValue && deadline.HasValue &&
            teamFormationEndsAtUtc.Value > deadline.Value)
            throw new BadRequestException("Формирование команд должно завершаться не позже дедлайна задания");
    }

    private static string ResolveStatus(Assignment assignment)
    {
        var now = DateTime.UtcNow;

        if (assignment.Deadline.HasValue && now > assignment.Deadline.Value)
            return AssignmentStatus.Finished;

        if (assignment.StartsAtUtc.HasValue && now < assignment.StartsAtUtc.Value)
            return AssignmentStatus.Hidden;

        return AssignmentStatus.Available;
    }
    
    public async Task<AssignmentDto> CreateAsync(CreateAssignmentRequest dto)
    {
        var userId = _currentUser.GetUserId();

        await EnsureTeacherOrAdmin(dto.CourseId);
        var teamFormationMode = ParseTeamFormationMode(dto.TeamFormationMode);
        ValidateAssignmentSchedule(dto.StartsAtUtc, dto.Deadline);
        ValidateTeamSize(dto.MinTeamSize, dto.MaxTeamSize);
        ValidateTeamFormationSchedule(dto.StartsAtUtc, dto.CaptainSelectionEndsAtUtc, dto.TeamFormationEndsAtUtc, dto.Deadline);
        
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
            Deadline = dto.Deadline,
            Created = DateTime.UtcNow
        };

        await _assignmentRepository.AddAsync(assignment);

        return Map(assignment);
    }

    public async Task<AssignmentDto> GetByIdAsync(Guid id)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Задание не найдено");

        await EnsureAssignmentVisibleToCurrentUser(assignment);
        
        return MapAssignment(assignment);
    }

    public async Task<AssignmentDto> UpdateAsync(Guid id, UpdateAssignmentRequest dto)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Задание не найдено");

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

        ValidateAssignmentSchedule(nextStartsAtUtc, nextDeadline);
        ValidateTeamSize(nextMinTeamSize, nextMaxTeamSize);
        ValidateTeamFormationSchedule(nextStartsAtUtc, nextCaptainSelectionEndsAtUtc, nextTeamFormationEndsAtUtc, nextDeadline);
        
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

        if (dto.RequiresSubmission.HasValue)
            assignment.RequiresSubmission = dto.RequiresSubmission.Value;

        if (dto.Deadline.HasValue)
            assignment.Deadline = dto.Deadline;

        await _assignmentRepository.UpdateAsync(assignment);

        return Map(assignment);
    }

    public async Task DeleteAsync(Guid id)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Задание не найдено");

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
            RequiresSubmission = assignment.RequiresSubmission,
            Deadline = assignment.Deadline,
            Created = assignment.Created
        };
    }
    
    public async Task<List<AssignmentFileDto>> AddFileAsync(Guid assignmentId, AssignmentFilesFormRequest dto)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId)
                         ?? throw new NotFoundException("Задание не найдено");

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
                         ?? throw new NotFoundException("Задание не найдено");

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
                var now = DateTime.UtcNow;
                items = items
                    .Where(x => !x.StartsAtUtc.HasValue || x.StartsAtUtc.Value <= now)
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
            RequiresSubmission = assignment.RequiresSubmission,
            Deadline = assignment.Deadline,
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
}
