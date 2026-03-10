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
        
        return Map(assignment);
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
    
    public async Task<AssignmentFileDto> AddFileAsync(Guid assignmentId, AssignmentFileFormRequest dto)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId)
                         ?? throw new NotFoundException("Задание не найдено");

        await EnsureTeacherOrAdmin(assignment.CourseId);

        byte[] content;
        await using (var ms = new MemoryStream())
        {
            await dto.Content.CopyToAsync(ms);
            content = ms.ToArray();
        }

        var url = await _fileStorage.UploadFileAsync(content, dto.Content.FileName, dto.Content.ContentType);

        var assignmentFile = new AssignmentFile
        {
            Id = Guid.NewGuid(),
            AssignmentId = assignmentId,
            FileName = dto.Content.FileName,
            Url = url,
            MimeType = dto.Content.ContentType,
            Size = content.LongLength,
            Type = dto.Type
        };

        await _assignmentRepository.AddFileAsync(assignmentFile);

        return new AssignmentFileDto
        {
            Id = assignmentFile.Id,
            FileName = assignmentFile.FileName,
            Url = assignmentFile.Url,
            MimeType = assignmentFile.MimeType,
            Size = assignmentFile.Size,
            Type = assignmentFile.Type
        };
    }

    public async Task DeleteFileAsync(Guid assignmentId, Guid fileId)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId)
                         ?? throw new NotFoundException("Задание не найдено");

        await EnsureTeacherOrAdmin(assignment.CourseId);

        var file = assignment.Files.FirstOrDefault(f => f.Id == fileId)
                   ?? throw new NotFoundException("Файл не найден");

        await _fileStorage.DeleteFileAsync(file.Url);

        assignment.Files.Remove(file);
        await _assignmentRepository.UpdateAsync(assignment);
    }

    private static AssignmentDto MapAssigment(Assignment a) => new()
    {
        Id = a.Id,
        CourseId = a.CourseId,
        Title = a.Title,
        Text = a.Text,
        RequiresSubmission = a.RequiresSubmission,
        Deadline = a.Deadline,
        Created = a.Created
    };
}