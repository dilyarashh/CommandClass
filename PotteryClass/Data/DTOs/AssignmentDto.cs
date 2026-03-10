namespace PotteryClass.Data.DTOs;

public class AssignmentDto
{
    public Guid Id { get; set; }

    public Guid CourseId { get; set; }

    public string Title { get; set; }

    public string Text { get; set; }

    public bool RequiresSubmission { get; set; }

    public DateTime? Deadline { get; set; }

    public DateTime Created { get; set; }
}