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
        
        var bytes = new byte[] { 1, 2, 3 };
        var stream = new MemoryStream(bytes);

        IFormFile file = new FormFile(stream, 0, bytes.Length, "Content", "test.png")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/png"
        };

        var dto = new AssignmentFilesFormRequest
        {
            Files = new List<AssignmentFileFormRequest>
            {
                new AssignmentFileFormRequest
                {
                    File = file
                }
            }
        };

        _fileStorage.Setup(f => f.UploadFileAsync(bytes, file.FileName, file.ContentType))
            .ReturnsAsync("https://minio.local/test.png");

        var service = CreateService();

        var result = await service.AddFileAsync(assignmentId, dto);

        result.Should().HaveCount(1);
        result[0].FileName.Should().Be("test.png");
        result[0].Url.Should().Be("https://minio.local/test.png");

        _assignmentRepo.Verify(r => r.AddFileAsync(It.IsAny<AssignmentFile>()), Times.Once);
    }
    
    [Fact]
    public async Task DeleteFileAsync_Should_Remove_File_From_Assignment_And_MinIO()
    {
        var assignmentId = Guid.NewGuid();
        Guid fileId = Guid.NewGuid();

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

        await service.DeleteFileAsync(assignmentId, new List<Guid> { fileId });

        assignment.Files.Should().BeEmpty();

        _fileStorage.Verify(f => f.DeleteFileAsync("https://minio.local/test.png"), Times.Once);
        _assignmentRepo.Verify(r => r.UpdateAsync(assignment), Times.Once);
    }
}