using FluentAssertions;
using Moq;
using PotteryClass.Data.DTOs;
using PotteryClass.Data.Entities;
using PotteryClass.Data.Entities.Enums;
using PotteryClass.Data.Repositories;
using PotteryClass.Infrastructure.Auth;
using PotteryClass.Services;
using Xunit;

namespace PotteryClassTests;

public class AssignmentFileServiceTests
{
    private readonly Mock<IAssignmentRepository> _assignmentRepo = new();
    private readonly Mock<ICourseTeacherRepository> _teacherRepo = new();
    private readonly Mock<ICourseStudentRepository> _studentRepo = new();
    private readonly Mock<ICurrentUser> _currentUser = new();
    private readonly Mock<IFileStorageService> _fileStorage = new();

    private AssignmentService CreateService()
        => new(_assignmentRepo.Object, _teacherRepo.Object, _studentRepo.Object, _currentUser.Object, _fileStorage.Object);

    [Fact]
    public async Task AddFileAsync_Should_Add_File_To_Assignment()
    {
        var assignmentId = Guid.NewGuid();
        var assignment = new Assignment { Id = assignmentId };

        _assignmentRepo.Setup(r => r.GetByIdAsync(assignmentId))
            .ReturnsAsync(assignment);

        _currentUser.Setup(x => x.GetRole()).Returns(UserRole.Admin);
        _currentUser.Setup(x => x.GetUserId()).Returns(Guid.NewGuid());

        var dto = new AssignmentFileRequest
        {
            FileName = "test.png",
            Content = new byte[] { 1, 2, 3 },
            MimeType = "image/png",
            Type = FileType.Image
        };

        _fileStorage.Setup(f => f.UploadFileAsync(dto.Content, dto.FileName, dto.MimeType))
            .ReturnsAsync("https://minio.local/test.png");

        var service = CreateService();

        var result = await service.AddFileAsync(assignmentId, dto);

        result.FileName.Should().Be("test.png");
        result.Url.Should().Be("https://minio.local/test.png");

        _assignmentRepo.Verify(r => r.UpdateAsync(assignment), Times.Once);
    }
    
    [Fact]
    public async Task DeleteFileAsync_Should_Remove_File_From_Assignment_And_MinIO()
    {
        var assignmentId = Guid.NewGuid();
        var fileId = Guid.NewGuid();

        var file = new AssignmentFile
        {
            Id = fileId,
            Url = "https://minio.local/test.png"
        };

        var assignment = new Assignment
        {
            Id = assignmentId,
            Files = new List<AssignmentFile> { file }
        };

        _assignmentRepo.Setup(r => r.GetByIdAsync(assignmentId))
            .ReturnsAsync(assignment);

        _currentUser.Setup(x => x.GetRole()).Returns(UserRole.Admin);
        _currentUser.Setup(x => x.GetUserId()).Returns(Guid.NewGuid());

        var service = CreateService();

        await service.DeleteFileAsync(assignmentId, fileId);

        assignment.Files.Should().BeEmpty();

        _fileStorage.Verify(f => f.DeleteFileAsync("https://minio.local/test.png"), Times.Once);
        _assignmentRepo.Verify(r => r.UpdateAsync(assignment), Times.Once);
    }
}