using Microsoft.EntityFrameworkCore;
using PotteryClass.Data.Entities;
using PotteryClass.Data.DTOs;

namespace PotteryClass.Data.Repositories;

public class SubmissionRepository(AppDbContext db) : ISubmissionRepository
{
    public async Task<Submission?> GetByIdAsync(Guid submissionId)
    {
        return await db.Submissions
            .Include(x => x.Files)
            .Include(x => x.Student)
            .Include(x => x.Assessment)
            .FirstOrDefaultAsync(x => x.Id == submissionId);
    }

    public async Task SaveChangesAsync()
    {
        await db.SaveChangesAsync();
    }
    
    public async Task AddAsync(Submission submission)
    {
        db.Submissions.Add(submission);
        await db.SaveChangesAsync();
    }

    public async Task<Submission?> GetByAssignmentAndStudentAsync(Guid assignmentId, Guid studentId)
    {
        return await db.Submissions
            .Include(x => x.Files)
            .Include(x => x.Assessment)
            .FirstOrDefaultAsync(x =>
                x.AssignmentId == assignmentId &&
                x.StudentId == studentId);
    }

    public async Task UpdateAsync(Submission submission)
    {
        db.Submissions.Update(submission);
        await db.SaveChangesAsync();
    }
    
    public async Task<List<Submission>> GetByAssignmentAsync(Guid assignmentId)
    {
        return await db.Submissions
            .Include(x => x.Files)
            .Include(x => x.Student)
            .Include(x => x.Assessment)
            .Where(x => x.AssignmentId == assignmentId)
            .OrderByDescending(x => x.Created)
            .ToListAsync();
    }

    public async Task<List<Submission>> GetByAssignmentAndStudentsAsync(Guid assignmentId, IReadOnlyCollection<Guid> studentIds)
    {
        return await db.Submissions
            .Include(x => x.Files)
            .Include(x => x.Student)
            .Include(x => x.Assessment)
            .Where(x => x.AssignmentId == assignmentId && studentIds.Contains(x.StudentId))
            .OrderByDescending(x => x.Created)
            .ToListAsync();
    }

    public async Task<List<CourseStudentGradeDto>> GetCourseGradesAsync(Guid courseId)
    {
        var submissions = await db.Submissions
            .Include(x => x.Assessment)
            .Include(x => x.Student)
            .Join(
                db.Assignments,
                submission => submission.AssignmentId,
                assignment => assignment.Id,
                (submission, assignment) => new { submission, assignment })
            .Where(x => x.assignment.CourseId == courseId)
            .ToListAsync();

        return submissions
            .Select(x => new CourseStudentGradeDto
            {
                StudentId = x.submission.StudentId,
                StudentName = x.submission.Student.FirstName + " " + x.submission.Student.LastName,
                AssignmentId = x.submission.AssignmentId,
                AssignmentTitle = x.assignment.Title,
                Grade = ResolveGrade(x.submission),
                CalculatedGrade = x.submission.Assessment?.FinalGrade
            })
            .ToList();
    }

    public async Task<List<MyCourseGradeDto>> GetStudentCourseGradesAsync(Guid courseId, Guid studentId)
    {
        var submissions = await db.Submissions
            .Include(x => x.Assessment)
            .Join(
                db.Assignments,
                submission => submission.AssignmentId,
                assignment => assignment.Id,
                (submission, assignment) => new { submission, assignment })
            .Where(x => x.assignment.CourseId == courseId && x.submission.StudentId == studentId)
            .ToListAsync();

        return submissions
            .Select(x => new MyCourseGradeDto
            {
                AssignmentId = x.submission.AssignmentId,
                AssignmentTitle = x.assignment.Title,
                Grade = ResolveGrade(x.submission),
                CalculatedGrade = x.submission.Assessment?.FinalGrade
            })
            .ToList();
    }

    private static int? ResolveGrade(Submission submission)
    {
        return submission.Assessment is null
            ? submission.Grade
            : decimal.ToInt32(decimal.Round(submission.Assessment.FinalGrade, 0, MidpointRounding.AwayFromZero));
    }
}
