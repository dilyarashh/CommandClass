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
            .Where(x => x.AssignmentId == assignmentId)
            .OrderByDescending(x => x.Created)
            .ToListAsync();
    }

    public async Task<List<CourseStudentGradeDto>> GetCourseGradesAsync(Guid courseId)
    {
        return await (
            from s in db.Submissions
            join a in db.Assignments on s.AssignmentId equals a.Id
            join u in db.Users on s.StudentId equals u.Id
            where a.CourseId == courseId
            select new CourseStudentGradeDto
            {
                StudentId = s.StudentId,
                StudentName = u.FirstName + " " + u.LastName,
                AssignmentId = s.AssignmentId,
                AssignmentTitle = a.Title,
                Grade = s.Grade
            })
            .ToListAsync();
    }

    public async Task<List<MyCourseGradeDto>> GetStudentCourseGradesAsync(Guid courseId, Guid studentId)
    {
        return await (
            from s in db.Submissions
            join a in db.Assignments on s.AssignmentId equals a.Id
            where a.CourseId == courseId && s.StudentId == studentId
            select new MyCourseGradeDto
            {
                AssignmentId = s.AssignmentId,
                AssignmentTitle = a.Title,
                Grade = s.Grade
            })
            .ToListAsync();
    }
}