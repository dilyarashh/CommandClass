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
    /// Тесты на выставление оценки решению

    [Fact]
    public async Task SetGrade_WhenSubmissionNotExists_ThrowsNotFound()
    {
        var submissionRepo = new Mock<ISubmissionRepository>();
        var assignmentRepo = new Mock<IAssignmentRepository>();
        var courseRepo = new Mock<ICourseRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var submissionId = Guid.NewGuid();

        submissionRepo.Setup(x => x.GetByIdAsync(submissionId))
            .ReturnsAsync((Submission?)null);

        var service = new GradeService(
            submissionRepo.Object,
            assignmentRepo.Object,
            courseRepo.Object,
            currentUser.Object);

        Func<Task> act = () => service.SetGradeAsync(
            submissionId,
            new SetSubmissionGradeRequest
            {
                Value = 5
            });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task SetGrade_WhenAssignmentNotExists_ThrowsNotFound()
    {
        var submissionRepo = new Mock<ISubmissionRepository>();
        var assignmentRepo = new Mock<IAssignmentRepository>();
        var courseRepo = new Mock<ICourseRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var submissionId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();

        submissionRepo.Setup(x => x.GetByIdAsync(submissionId))
            .ReturnsAsync(new Submission
            {
                Id = submissionId,
                AssignmentId = assignmentId,
                StudentId = Guid.NewGuid()
            });

        assignmentRepo.Setup(x => x.GetByIdAsync(assignmentId))
            .ReturnsAsync((Assignment?)null);

        var service = new GradeService(
            submissionRepo.Object,
            assignmentRepo.Object,
            courseRepo.Object,
            currentUser.Object);

        Func<Task> act = () => service.SetGradeAsync(
            submissionId,
            new SetSubmissionGradeRequest
            {
                Value = 5
            });

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task SetGrade_WhenUserNotTeacher_ThrowsForbidden()
    {
        var submissionRepo = new Mock<ISubmissionRepository>();
        var assignmentRepo = new Mock<IAssignmentRepository>();
        var courseRepo = new Mock<ICourseRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var teacherId = Guid.NewGuid();
        var submissionId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId())
            .Returns(teacherId);

        submissionRepo.Setup(x => x.GetByIdAsync(submissionId))
            .ReturnsAsync(new Submission
            {
                Id = submissionId,
                AssignmentId = assignmentId,
                StudentId = Guid.NewGuid()
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
                Teachers = new List<CourseTeacher>(),
                Students = new List<CourseStudent>()
            });

        var service = new GradeService(
            submissionRepo.Object,
            assignmentRepo.Object,
            courseRepo.Object,
            currentUser.Object);

        Func<Task> act = () => service.SetGradeAsync(
            submissionId,
            new SetSubmissionGradeRequest
            {
                Value = 5
            });

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task SetGrade_WhenValid_SetsGrade()
    {
        var submissionRepo = new Mock<ISubmissionRepository>();
        var assignmentRepo = new Mock<IAssignmentRepository>();
        var courseRepo = new Mock<ICourseRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var teacherId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var submissionId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        var submission = new Submission
        {
            Id = submissionId,
            AssignmentId = assignmentId,
            StudentId = studentId,
            Grade = null
        };

        currentUser.Setup(x => x.GetUserId())
            .Returns(teacherId);

        submissionRepo.Setup(x => x.GetByIdAsync(submissionId))
            .ReturnsAsync(submission);

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
                },
                Students = new List<CourseStudent>
                {
                    new() { UserId = studentId, IsBlocked = false }
                }
            });

        submissionRepo.Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var service = new GradeService(
            submissionRepo.Object,
            assignmentRepo.Object,
            courseRepo.Object,
            currentUser.Object);

        var result = await service.SetGradeAsync(
            submissionId,
            new SetSubmissionGradeRequest
            {
                Value = 5
            });

        result.SubmissionId.Should().Be(submissionId);
        result.StudentId.Should().Be(studentId);
        result.Grade.Should().Be(5);

        submission.Grade.Should().Be(5);
        submission.GradedByTeacherId.Should().Be(teacherId);
        submission.GradedAtUtc.Should().NotBeNull();

        submissionRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
    }


    /// Тесты на удаление оценки у решения

    [Fact]
    public async Task DeleteGrade_WhenSubmissionNotExists_ThrowsNotFound()
    {
        var submissionRepo = new Mock<ISubmissionRepository>();
        var assignmentRepo = new Mock<IAssignmentRepository>();
        var courseRepo = new Mock<ICourseRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var submissionId = Guid.NewGuid();

        submissionRepo.Setup(x => x.GetByIdAsync(submissionId))
            .ReturnsAsync((Submission?)null);

        var service = new GradeService(
            submissionRepo.Object,
            assignmentRepo.Object,
            courseRepo.Object,
            currentUser.Object);

        Func<Task> act = () => service.DeleteGradeAsync(submissionId);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task DeleteGrade_WhenUserNotTeacher_ThrowsForbidden()
    {
        var submissionRepo = new Mock<ISubmissionRepository>();
        var assignmentRepo = new Mock<IAssignmentRepository>();
        var courseRepo = new Mock<ICourseRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var teacherId = Guid.NewGuid();
        var submissionId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        currentUser.Setup(x => x.GetUserId())
            .Returns(teacherId);

        submissionRepo.Setup(x => x.GetByIdAsync(submissionId))
            .ReturnsAsync(new Submission
            {
                Id = submissionId,
                AssignmentId = assignmentId,
                StudentId = Guid.NewGuid(),
                Grade = 5
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
                Teachers = new List<CourseTeacher>(),
                Students = new List<CourseStudent>()
            });

        var service = new GradeService(
            submissionRepo.Object,
            assignmentRepo.Object,
            courseRepo.Object,
            currentUser.Object);

        Func<Task> act = () => service.DeleteGradeAsync(submissionId);

        await act.Should().ThrowAsync<ForbiddenException>();
    }

    [Fact]
    public async Task DeleteGrade_WhenUserIsTeacher_RemovesGrade()
    {
        var submissionRepo = new Mock<ISubmissionRepository>();
        var assignmentRepo = new Mock<IAssignmentRepository>();
        var courseRepo = new Mock<ICourseRepository>();
        var currentUser = new Mock<ICurrentUser>();

        var teacherId = Guid.NewGuid();
        var studentId = Guid.NewGuid();
        var submissionId = Guid.NewGuid();
        var assignmentId = Guid.NewGuid();
        var courseId = Guid.NewGuid();

        var submission = new Submission
        {
            Id = submissionId,
            AssignmentId = assignmentId,
            StudentId = studentId,
            Grade = 5,
            GradedByTeacherId = teacherId,
            GradedAtUtc = DateTime.UtcNow
        };

        currentUser.Setup(x => x.GetUserId())
            .Returns(teacherId);

        submissionRepo.Setup(x => x.GetByIdAsync(submissionId))
            .ReturnsAsync(submission);

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

        submissionRepo.Setup(x => x.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        var service = new GradeService(
            submissionRepo.Object,
            assignmentRepo.Object,
            courseRepo.Object,
            currentUser.Object);

        await service.DeleteGradeAsync(submissionId);

        submission.Grade.Should().BeNull();
        submission.GradedByTeacherId.Should().BeNull();
        submission.GradedAtUtc.Should().BeNull();

        submissionRepo.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}