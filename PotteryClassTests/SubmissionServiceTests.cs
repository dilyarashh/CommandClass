using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using PotteryClass.Data.DTOs;
using PotteryClass.Data.Entities;
using PotteryClass.Data.Entities.Enums;
using PotteryClass.Data.Repositories;
using PotteryClass.Infrastructure.Auth;
using PotteryClass.Services;
using Xunit;

namespace PotteryClassTests;

public class SubmissionServiceTests
{
    private readonly Mock<ISubmissionRepository> _submissionRepo = new();
    private readonly Mock<IAssignmentRepository> _assignmentRepo = new();
    private readonly Mock<ICourseStudentRepository> _studentRepo = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IFileStorageService> _fileStorage = new();

    private SubmissionService CreateService()
        => new(
            _submissionRepo.Object,
            _assignmentRepo.Object,
            _studentRepo.Object,
            _currentUser.Object,
            _fileStorage.Object);

    [Fact]
    public async Task SubmitAsync_Should_Create_Submission_With_File()
    {
        var assignmentId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        var assignment = new Assignment
        {
            Id = assignmentId,
            CourseId = courseId
        };

        _assignmentRepo.Setup(x => x.GetByIdAsync(assignmentId))
            .ReturnsAsync(assignment);

        _studentRepo.Setup(x => x.IsStudentAsync(courseId, studentId))
            .ReturnsAsync(true);

        _currentUser.Setup(x => x.GetUserId()).Returns(studentId);
        _currentUser.Setup(x => x.GetRole()).Returns(UserRole.Student);

        var bytes = new byte[] { 1, 2, 3 };
        var stream = new MemoryStream(bytes);

        IFormFile file = new FormFile(stream, 0, bytes.Length, "Content", "solution.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };

        var dto = new SubmissionFilesFormRequest
        {
            Files = new List<SubmissionFileFormRequest>
            {
                new()
                {
                    File = file
                }
            }
        };

        _fileStorage.Setup(x =>
                x.UploadFileAsync(It.IsAny<byte[]>(), file.FileName, file.ContentType))
            .ReturnsAsync("https://minio.local/solution.png");

        var service = CreateService();

        var result = await service.SubmitAsync(assignmentId, dto);

        result.AssignmentId.Should().Be(assignmentId);
        result.StudentId.Should().Be(studentId);
        result.Files.Should().HaveCount(1);
        result.Files.First().FileName.Should().Be("solution.png");

        _submissionRepo.Verify(x => x.AddAsync(It.IsAny<Submission>()), Times.Once);
    }

    [Fact]
    public async Task SubmitAsync_Should_Upload_Multiple_Files()
    {
        var assignmentId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        var assignment = new Assignment
        {
            Id = assignmentId,
            CourseId = courseId
        };

        _assignmentRepo.Setup(x => x.GetByIdAsync(assignmentId))
            .ReturnsAsync(assignment);

        _studentRepo.Setup(x => x.IsStudentAsync(courseId, studentId))
            .ReturnsAsync(true);

        _currentUser.Setup(x => x.GetUserId()).Returns(studentId);
        _currentUser.Setup(x => x.GetRole()).Returns(UserRole.Student);

        var bytes = new byte[] { 1, 2, 3 };

        var file1 = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file1", "a.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };

        var file2 = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file2", "b.pdf")
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/pdf"
        };

        var dto = new SubmissionFilesFormRequest
        {
            Files = new List<SubmissionFileFormRequest>
            {
                new() { File = file1 },
                new() { File = file2 }
            }
        };

        _fileStorage.Setup(x =>
                x.UploadFileAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("https://minio.local/file");

        var service = CreateService();

        var result = await service.SubmitAsync(assignmentId, dto);

        result.Files.Should().HaveCount(2);

        _submissionRepo.Verify(x => x.AddAsync(It.IsAny<Submission>()), Times.Once);
    }

    [Fact]
    public async Task DeleteFilesAsync_Should_Remove_File_From_Submission()
    {
        var submissionId = Guid.NewGuid();
        var studentId = Guid.NewGuid();

        var fileId = Guid.NewGuid();

        var file = new SubmissionFile
        {
            Id = fileId,
            Url = "https://minio.local/test.png"
        };

        var submission = new Submission
        {
            Id = submissionId,
            StudentId = studentId,
            Files = new List<SubmissionFile> { file }
        };

        _submissionRepo.Setup(x => x.GetByIdAsync(submissionId))
            .ReturnsAsync(submission);

        _currentUser.Setup(x => x.GetUserId()).Returns(studentId);
        _currentUser.Setup(x => x.GetRole()).Returns(UserRole.Student);

        var service = CreateService();

        await service.DeleteFilesAsync(submissionId, new List<Guid> { fileId });

        submission.Files.Should().BeEmpty();

        _fileStorage.Verify(x => x.DeleteFileAsync(file.Url), Times.Once);
        _submissionRepo.Verify(x => x.UpdateAsync(submission), Times.Once);
    }

    [Fact]
    public async Task DeleteFilesAsync_Should_Remove_Multiple_Files()
    {
        var submissionId = Guid.NewGuid();
        var studentId = Guid.NewGuid();

        var file1 = new SubmissionFile
        {
            Id = Guid.NewGuid(),
            Url = "https://minio.local/a.png"
        };

        var file2 = new SubmissionFile
        {
            Id = Guid.NewGuid(),
            Url = "https://minio.local/b.png"
        };

        var submission = new Submission
        {
            Id = submissionId,
            StudentId = studentId,
            Files = new List<SubmissionFile> { file1, file2 }
        };

        _submissionRepo.Setup(x => x.GetByIdAsync(submissionId))
            .ReturnsAsync(submission);

        _currentUser.Setup(x => x.GetUserId()).Returns(studentId);
        _currentUser.Setup(x => x.GetRole()).Returns(UserRole.Student);

        var service = CreateService();

        await service.DeleteFilesAsync(submissionId, new List<Guid> { file1.Id, file2.Id });

        submission.Files.Should().BeEmpty();

        _fileStorage.Verify(x => x.DeleteFileAsync(It.IsAny<string>()), Times.Exactly(2));
        _submissionRepo.Verify(x => x.UpdateAsync(submission), Times.Once);
    }
    
    [Fact]
    public async Task GetAssignmentSubmissionsAsync_Should_Return_List()
    {
        var assignmentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        var assignment = new Assignment
        {
            Id = assignmentId,
            CourseId = courseId
        };

        _assignmentRepo.Setup(x => x.GetByIdAsync(assignmentId))
            .ReturnsAsync(assignment);

        var submissions = new List<Submission>
        {
            new()
            {
                Id = Guid.NewGuid(),
                AssignmentId = assignmentId,
                StudentId = Guid.NewGuid(),
                Created = DateTime.UtcNow,
                Status = SubmissionStatus.Submitted
            },
            new()
            {
                Id = Guid.NewGuid(),
                AssignmentId = assignmentId,
                StudentId = Guid.NewGuid(),
                Created = DateTime.UtcNow,
                Status = SubmissionStatus.Submitted
            }
        };

        _submissionRepo.Setup(x => x.GetByAssignmentAsync(assignmentId))
            .ReturnsAsync(submissions);

        _currentUser.Setup(x => x.GetRole()).Returns(UserRole.Admin);

        var service = CreateService();

        var result = await service.GetAssignmentSubmissionsAsync(assignmentId);

        result.Should().HaveCount(2);
    }
    
    [Fact]
    public async Task GetByIdAsync_Should_Return_Submission()
    {
        var submissionId = Guid.NewGuid();

        var submission = new Submission
        {
            Id = submissionId,
            AssignmentId = Guid.NewGuid(),
            StudentId = Guid.NewGuid(),
            Created = DateTime.UtcNow,
            Status = SubmissionStatus.Submitted
        };

        _submissionRepo.Setup(x => x.GetByIdAsync(submissionId))
            .ReturnsAsync(submission);

        _currentUser.Setup(x => x.GetRole()).Returns(UserRole.Admin);

        var service = CreateService();

        var result = await service.GetByIdAsync(submissionId);

        result.Id.Should().Be(submissionId);
    }
    
    [Fact]
    public async Task GetMySubmissionAsync_Should_Return_Current_User_Submission()
    {
        var assignmentId = Guid.NewGuid();
        var studentId = Guid.NewGuid();

        var submission = new Submission
        {
            Id = Guid.NewGuid(),
            AssignmentId = assignmentId,
            StudentId = studentId,
            Created = DateTime.UtcNow,
            Status = SubmissionStatus.Submitted
        };

        _submissionRepo.Setup(x => x.GetByAssignmentAndStudentAsync(assignmentId, studentId))
            .ReturnsAsync(submission);

        _currentUser.Setup(x => x.GetUserId()).Returns(studentId);
        _currentUser.Setup(x => x.GetRole()).Returns(UserRole.Student);

        var service = CreateService();

        var result = await service.GetMySubmissionAsync(assignmentId);

        result.AssignmentId.Should().Be(assignmentId);
        result.StudentId.Should().Be(studentId);
    }
}