using FluentAssertions;
using Xunit;
using Moq;
using PotteryClass.Data.DTOs;
using PotteryClass.Data.Entities;
using PotteryClass.Data.Repositories;
using PotteryClass.Infrastructure.Auth;
using PotteryClass.Infrastructure.Errors.Exceptions;
using PotteryClass.Services;

namespace PotteryClassTests;

public class GradeServiceTests
{

    /// Тесты на создание оценки

    [Fact]
    public async Task CreateGrade_WhenAssignmentNotExists_ThrowsNotFound()
    {
        var assignmentRepo = new Mock<IAssignmentRepository>();
        var gradeRepo = new Mock<IGradeRepository>();
        var courseRepo = new Mock<ICourseRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var assignmentId = Guid.NewGuid();

        assignmentRepo.Setup(x => x.GetByIdAsync(assignmentId))
            .ReturnsAsync((Assignment?)null);

        var service = new GradeService(
            assignmentRepo.Object,
            gradeRepo.Object,
            courseRepo.Object,
            currentUser.Object);

        Func<Task> act = () => service.CreateGradeAsync(
            assignmentId,
            new CreateGradeRequest
            {
                StudentId = Guid.NewGuid(),
                Value = 5
            });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateGrade_WhenUserNotTeacher_ThrowsForbidden()
    {
        var assignmentRepo = new Mock<IAssignmentRepository>();
        var gradeRepo = new Mock<IGradeRepository>();
        var courseRepo = new Mock<ICourseRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var assignmentId = Guid.NewGuid();
        var teacherId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId())
            .Returns(teacherId);

        var assignment = new Assignment
        {
            Id = assignmentId,
            CourseId = Guid.NewGuid()
        };

        assignmentRepo.Setup(x => x.GetByIdAsync(assignmentId))
            .ReturnsAsync(assignment);

        courseRepo.Setup(x => x.GetByIdAsync(assignment.CourseId))
            .ReturnsAsync(new Course
            {
                Id = assignment.CourseId,
                Teachers = new List<CourseTeacher>(),
                Students = new List<CourseStudent>()
            });

        var service = new GradeService(
            assignmentRepo.Object,
            gradeRepo.Object,
            courseRepo.Object,
            currentUser.Object);

        Func<Task> act = () => service.CreateGradeAsync(
            assignmentId,
            new CreateGradeRequest
            {
                StudentId = Guid.NewGuid(),
                Value = 5
            });

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task CreateGrade_WhenStudentNotInCourse_ThrowsNotFound()
    {
        var assignmentRepo = new Mock<IAssignmentRepository>();
        var gradeRepo = new Mock<IGradeRepository>();
        var courseRepo = new Mock<ICourseRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var teacherId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId())
            .Returns(teacherId);

        var assignment = new Assignment
        {
            Id = assignmentId,
            CourseId = Guid.NewGuid()
        };

        assignmentRepo.Setup(x => x.GetByIdAsync(assignmentId))
            .ReturnsAsync(assignment);

        courseRepo.Setup(x => x.GetByIdAsync(assignment.CourseId))
            .ReturnsAsync(new Course
            {
                Id = assignment.CourseId,
                Teachers = new List<CourseTeacher>
                {
                    new() { UserId = teacherId }
                },
                Students = new List<CourseStudent>()
            });

        var service = new GradeService(
            assignmentRepo.Object,
            gradeRepo.Object,
            courseRepo.Object,
            currentUser.Object);

        Func<Task> act = () => service.CreateGradeAsync(
            assignmentId,
            new CreateGradeRequest
            {
                StudentId = Guid.NewGuid(),
                Value = 5
            });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateGrade_WhenGradeAlreadyExists_ThrowsBadRequest()
    {
        var assignmentRepo = new Mock<IAssignmentRepository>();
        var gradeRepo = new Mock<IGradeRepository>();
        var courseRepo = new Mock<ICourseRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var teacherId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId())
            .Returns(teacherId);

        var assignment = new Assignment
        {
            Id = assignmentId,
            CourseId = Guid.NewGuid()
        };

        assignmentRepo.Setup(x => x.GetByIdAsync(assignmentId))
            .ReturnsAsync(assignment);

        courseRepo.Setup(x => x.GetByIdAsync(assignment.CourseId))
            .ReturnsAsync(new Course
            {
                Id = assignment.CourseId,
                Teachers = new List<CourseTeacher>
                {
                    new() { UserId = teacherId }
                },
                Students = new List<CourseStudent>
                {
                    new() { UserId = studentId, IsBlocked = false }
                }
            });

        gradeRepo.Setup(x => x.ExistsAsync(assignmentId, studentId))
            .ReturnsAsync(true);

        var service = new GradeService(
            assignmentRepo.Object,
            gradeRepo.Object,
            courseRepo.Object,
            currentUser.Object);

        Func<Task> act = () => service.CreateGradeAsync(
            assignmentId,
            new CreateGradeRequest
            {
                StudentId = studentId,
                Value = 5
            });

        await act.Should().ThrowAsync<BadRequestException>();
    }

    [Fact]
    public async Task CreateGrade_WhenValid_CreatesGrade()
    {
        var assignmentRepo = new Mock<IAssignmentRepository>();
        var gradeRepo = new Mock<IGradeRepository>();
        var courseRepo = new Mock<ICourseRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var teacherId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId())
            .Returns(teacherId);

        var assignment = new Assignment
        {
            Id = assignmentId,
            CourseId = Guid.NewGuid()
        };

        assignmentRepo.Setup(x => x.GetByIdAsync(assignmentId))
            .ReturnsAsync(assignment);

        courseRepo.Setup(x => x.GetByIdAsync(assignment.CourseId))
            .ReturnsAsync(new Course
            {
                Id = assignment.CourseId,
                Teachers = new List<CourseTeacher>
                {
                    new() { UserId = teacherId }
                },
                Students = new List<CourseStudent>
                {
                    new() { UserId = studentId, IsBlocked = false }
                }
            });

        gradeRepo.Setup(x => x.ExistsAsync(assignmentId, studentId))
            .ReturnsAsync(false);

        gradeRepo.Setup(x => x.AddAsync(It.IsAny<Grade>()))
            .Returns(Task.CompletedTask);

        gradeRepo.Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var service = new GradeService(
            assignmentRepo.Object,
            gradeRepo.Object,
            courseRepo.Object,
            currentUser.Object);

        var result = await service.CreateGradeAsync(
            assignmentId,
            new CreateGradeRequest
            {
                StudentId = studentId,
                Value = 5
            });

        result.StudentId.Should().Be(studentId);
        result.Value.Should().Be(5);

        gradeRepo.Verify(x => x.AddAsync(It.IsAny<Grade>()), Times.Once);
        gradeRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
    }


    /// Тесты на удаление оценки

    [Fact]
    public async Task DeleteGrade_WhenGradeNotExists_ThrowsNotFound()
    {
        var assignmentRepo = new Mock<IAssignmentRepository>();
        var gradeRepo = new Mock<IGradeRepository>();
        var courseRepo = new Mock<ICourseRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var gradeId = Guid.NewGuid();

        gradeRepo.Setup(x => x.GetByIdAsync(gradeId))
            .ReturnsAsync((Grade?)null);

        var service = new GradeService(
            assignmentRepo.Object,
            gradeRepo.Object,
            courseRepo.Object,
            currentUser.Object);

        Func<Task> act = () => service.DeleteGradeAsync(gradeId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteGrade_WhenUserNotTeacher_ThrowsForbidden()
    {
        var assignmentRepo = new Mock<IAssignmentRepository>();
        var gradeRepo = new Mock<IGradeRepository>();
        var courseRepo = new Mock<ICourseRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var teacherId = Guid.NewGuid();
        var gradeId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId())
            .Returns(teacherId);

        gradeRepo.Setup(x => x.GetByIdAsync(gradeId))
            .ReturnsAsync(new Grade
            {
                Id = gradeId,
                AssignmentId = Guid.NewGuid()
            });

        assignmentRepo.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new Assignment
            {
                CourseId = courseId
            });

        courseRepo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(new Course
            {
                Id = courseId,
                Teachers = new List<CourseTeacher>(),
                Students = new List<CourseStudent>()
            });

        var service = new GradeService(
            assignmentRepo.Object,
            gradeRepo.Object,
            courseRepo.Object,
            currentUser.Object);

        Func<Task> act = () => service.DeleteGradeAsync(gradeId);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task DeleteGrade_WhenUserIsTeacher_DeletesGrade()
    {
        var assignmentRepo = new Mock<IAssignmentRepository>();
        var gradeRepo = new Mock<IGradeRepository>();
        var courseRepo = new Mock<ICourseRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var teacherId = Guid.NewGuid();
        var gradeId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId())
            .Returns(teacherId);

        gradeRepo.Setup(x => x.GetByIdAsync(gradeId))
            .ReturnsAsync(new Grade
            {
                Id = gradeId,
                AssignmentId = assignmentId
            });

        assignmentRepo.Setup(x => x.GetByIdAsync(assignmentId))
            .ReturnsAsync(new Assignment
            {
                Id = assignmentId,
                CourseId = courseId
            });

        courseRepo.Setup(x => x.GetByIdAsync(courseId))
            .ReturnsAsync(new Course
            {
                Id = courseId,
                Teachers = new List<CourseTeacher>
                {
                new() { UserId = teacherId }
                }
            });

        var service = new GradeService(
            assignmentRepo.Object,
            gradeRepo.Object,
            courseRepo.Object,
            currentUser.Object);

        await service.DeleteGradeAsync(gradeId);

        gradeRepo.Verify(x => x.Delete(It.IsAny<Grade>()), Times.Once);
        gradeRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}