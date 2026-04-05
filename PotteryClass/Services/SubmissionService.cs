using PotteryClass.Data.DTOs;
using PotteryClass.Data.Entities;
using PotteryClass.Data.Entities.Enums;
using PotteryClass.Data.Repositories;
using PotteryClass.Infrastructure.Auth;
using PotteryClass.Infrastructure.Errors.Exceptions;

namespace PotteryClass.Services;

public class SubmissionService(
    ISubmissionRepository submissionRepository,
    IAssignmentRepository assignmentRepository,
    ICourseStudentRepository studentRepository,
    ICourseTeacherRepository teacherRepository,
    ICurrentUser currentUser,
    IFileStorageService fileStorage)
    : ISubmissionService
{
    private readonly ISubmissionRepository _submissionRepository = submissionRepository;
    private readonly IAssignmentRepository _assignmentRepository = assignmentRepository;
    private readonly ICourseStudentRepository _studentRepository = studentRepository;
    private readonly ICourseTeacherRepository _teacherRepository = teacherRepository;
    private readonly ICurrentUser _currentUser = currentUser;
    private readonly IFileStorageService _fileStorage = fileStorage;

    private async Task EnsureStudent(Guid courseId)
    {
        var role = _currentUser.GetRole();

        if (role == UserRole.Admin)
            return;

        var userId = _currentUser.GetUserId();

        var isStudent = await _studentRepository.IsStudentAsync(courseId, userId);

        if (!isStudent)
            throw new ForbiddenException("Нет доступа");
    }

    private static void EnsureAssignmentAvailableForSubmission(Assignment assignment)
    {
        var now = DateTime.UtcNow;

        if (assignment.StartsAtUtc.HasValue && now < assignment.StartsAtUtc.Value)
            throw new BadRequestException("Задание ещё не доступно для отправки решения");

        if (assignment.Deadline.HasValue && now > assignment.Deadline.Value)
            throw new BadRequestException("Дедлайн задания уже прошёл");
    }

    public async Task<SubmissionDto> SubmitAsync(Guid assignmentId, SubmissionFilesFormRequest dto)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId)
            ?? throw new NotFoundException("Задание не найдено");

        await EnsureStudent(assignment.CourseId);
        EnsureAssignmentAvailableForSubmission(assignment);

        var studentId = _currentUser.GetUserId();

        var submission = new Submission
        {
            Id = Guid.NewGuid(),
            AssignmentId = assignmentId,
            StudentId = studentId,
            Created = DateTime.UtcNow,
            Status = SubmissionStatus.Submitted
        };

        foreach (var fileDto in dto.Files)
        {
            byte[] content;

            await using (var ms = new MemoryStream())
            {
                await fileDto.File.CopyToAsync(ms);
                content = ms.ToArray();
            }

            var url = await _fileStorage.UploadFileAsync(
                content,
                fileDto.File.FileName,
                fileDto.File.ContentType);

            submission.Files.Add(new SubmissionFile
            {
                Id = Guid.NewGuid(),
                FileName = fileDto.File.FileName,
                Url = url,
                MimeType = fileDto.File.ContentType,
                Size = content.LongLength
            });
        }

        await _submissionRepository.AddAsync(submission);

        return Map(submission);
    }

    public async Task DeleteFilesAsync(Guid submissionId, List<Guid> fileIds)
    {
        var submission = await _submissionRepository.GetByIdAsync(submissionId)
            ?? throw new NotFoundException("Решение не найдено");

        var assignment = await _assignmentRepository.GetByIdAsync(submission.AssignmentId)
            ?? throw new NotFoundException("Задание не найдено");

        var studentId = _currentUser.GetUserId();

        if (submission.StudentId != studentId)
            throw new ForbiddenException("Нет доступа");

        EnsureAssignmentAvailableForSubmission(assignment);

        var files = submission.Files
            .Where(x => fileIds.Contains(x.Id))
            .ToList();

        foreach (var file in files)
        {
            await _fileStorage.DeleteFileAsync(file.Url);
            submission.Files.Remove(file);
        }

        await _submissionRepository.UpdateAsync(submission);
    }

    private static SubmissionDto Map(Submission submission)
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

    public async Task<List<SubmissionDto>> GetAssignmentSubmissionsAsync(Guid assignmentId)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId)
                         ?? throw new NotFoundException("Задание не найдено");

        var role = _currentUser.GetRole();

        if (role != UserRole.Admin && role != UserRole.Teacher)
            throw new ForbiddenException("Нет доступа");

        var submissions = await _submissionRepository.GetByAssignmentAsync(assignmentId);

        return submissions.Select(Map).ToList();
    }
    
    public async Task<SubmissionDto> GetByIdAsync(Guid submissionId)
    {
        var submission = await _submissionRepository.GetByIdAsync(submissionId)
                         ?? throw new NotFoundException("Решение не найдено");

        var role = _currentUser.GetRole();
        var userId = _currentUser.GetUserId();

        if (role == UserRole.Teacher)
        {
            var assignment = await _assignmentRepository.GetByIdAsync(submission.AssignmentId)
                ?? throw new NotFoundException("Задание не найдено");

            var isTeacher = await _teacherRepository.IsTeacherAsync(assignment.CourseId, userId);

            if (!isTeacher)
                throw new ForbiddenException("Нет доступа");

            return Map(submission);
        }

        if (role != UserRole.Admin &&
            submission.StudentId != userId)
            throw new ForbiddenException("Нет доступа");

        return Map(submission);
    }
    
    public async Task<SubmissionDto> GetMySubmissionAsync(Guid assignmentId)
    {
        var studentId = _currentUser.GetUserId();

        var submission = await _submissionRepository
                             .GetByAssignmentAndStudentAsync(assignmentId, studentId)
                         ?? throw new NotFoundException("Решение не найдено");

        return Map(submission);
    }
}
