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
    
    public List<AssignmentFileDto> Files { get; set; } = new();

    public AssignmentPermissionsDto Permissions { get; set; } = new();
}
