namespace PotteryClass.Data.DTOs;

public class CreateAssignmentRequest
{
    public Guid CourseId { get; set; }
    
    public string Title { get; set; }
    
    public string Text { get; set; }

    public DateTime? PublishAtUtc { get; set; }

    public DateTime? StartsAtUtc { get; set; }

    public int? MinTeamSize { get; set; }

    public int? MaxTeamSize { get; set; }
    
    public bool RequiresSubmission { get; set; }
    
    public DateTime? Deadline { get; set; }
}
