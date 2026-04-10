using PotteryClass.Data.Entities.Enums;

namespace PotteryClass.Data.DTOs;

public class SubmissionDto
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public Guid StudentId { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? MiddleName { get; set; }

    public DateTime Created { get; set; }
    public int? Grade { get; set; }
    public string? TeacherComment { get; set; }
    public Guid? GradedByTeacherId { get; set; }
    public DateTime? GradedAtUtc { get; set; }
    public SubmissionStatus Status { get; set; }

    public List<SubmissionFileDto> Files { get; set; } = new();
}
