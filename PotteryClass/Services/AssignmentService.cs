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
    
    public async Task<AssignmentDto> CreateAsync(CreateAssignmentRequest dto)
    {
        var userId = _currentUser.GetUserId();

        await EnsureTeacherOrAdmin(dto.CourseId);
        
        var assignment = new Assignment
        {
            Id = Guid.NewGuid(),
            CourseId = dto.CourseId,
            CreatedById = userId,
            Title = dto.Title,
            Text = dto.Text,
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

        await EnsureCourseMember(assignment.CourseId);
        
        return MapAssignment(assignment);
    }

    public async Task<AssignmentDto> UpdateAsync(Guid id, UpdateAssignmentRequest dto)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(id)
            ?? throw new NotFoundException("Задание не найдено");

        await EnsureTeacherOrAdmin(assignment.CourseId);
        
        if (dto.Title is not null)
            assignment.Title = dto.Title;

        if (dto.Text is not null)
            assignment.Text = dto.Text;

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
    
    private static AssignmentDto MapAssignment(Assignment assignment)
    {
        return new AssignmentDto
        {
            Id = assignment.Id,
            CourseId = assignment.CourseId,
            Title = assignment.Title,
            Text = assignment.Text,
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