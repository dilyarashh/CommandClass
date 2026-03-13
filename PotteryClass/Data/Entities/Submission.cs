using PotteryClass.Data.Entities.Enums;

namespace PotteryClass.Data.Entities;

public class Submission
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public Guid StudentId { get; set; }
    public User Student { get; set; } = null!;
    public DateTime Created { get; set; }
    public int? Grade { get; set; }
    public Guid? GradedByTeacherId { get; set; }
    public DateTime? GradedAtUtc { get; set; }
    public SubmissionStatus Status { get; set; }
    public ICollection<SubmissionFile> Files { get; set; } = new List<SubmissionFile>();
}