namespace PotteryClass.Data.Entities;

public class Assignment
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public Guid CreatedById { get; set; }
    public string Title { get; set; }
    public string Text { get; set; }
    public DateTime? StartsAtUtc { get; set; }
    public int? MinTeamSize { get; set; }
    public int? MaxTeamSize { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime Created { get; set; }
    public bool RequiresSubmission { get; set; }
    
    public ICollection<AssignmentFile> Files { get; set; } = new List<AssignmentFile>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}
