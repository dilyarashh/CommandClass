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
    ICurrentUser currentUser,
    IFileStorageService fileStorage)
    : ISubmissionService
{
    private readonly ISubmissionRepository _submissionRepository = submissionRepository;
    private readonly IAssignmentRepository _assignmentRepository = assignmentRepository;
    private readonly ICourseStudentRepository _studentRepository = studentRepository;
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

    public async Task<SubmissionDto> SubmitAsync(Guid assignmentId, SubmissionFilesFormRequest dto)
    {
        var assignment = await _assignmentRepository.GetByIdAsync(assignmentId)
            ?? throw new NotFoundException("Задание не найдено");

        await EnsureStudent(assignment.CourseId);

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
                Size = content.LongLength,
                Type = fileDto.Type
            });
        }

        await _submissionRepository.AddAsync(submission);

        return Map(submission);
    }

    public async Task DeleteFilesAsync(Guid submissionId, List<Guid> fileIds)
    {
        var submission = await _submissionRepository.GetByIdAsync(submissionId)
            ?? throw new NotFoundException("Решение не найдено");

        var studentId = _currentUser.GetUserId();

        if (submission.StudentId != studentId)
            throw new ForbiddenException("Нет доступа");

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
}