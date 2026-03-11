namespace PotteryClass.Data.DTOs;

public class SubmissionGradeDto
{
    public Guid SubmissionId { get; set; }
    public Guid AssignmentId { get; set; }
    public Guid StudentId { get; set; }
    public int? Grade { get; set; }
    public Guid? GradedByTeacherId { get; set; }
    public DateTime? GradedAtUtc { get; set; }
}