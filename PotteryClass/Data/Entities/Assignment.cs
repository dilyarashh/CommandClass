namespace PotteryClass.Data.Entities;

using PotteryClass.Data.Entities.Enums;

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
    public AssignmentTeamFormationMode TeamFormationMode { get; set; } = AssignmentTeamFormationMode.TeacherManaged;
    public DateTime? CaptainSelectionEndsAtUtc { get; set; }
    public DateTime? TeamFormationEndsAtUtc { get; set; }
    public Guid? DraftCurrentCaptainUserId { get; set; }
    public DateTime? DraftStartedAtUtc { get; set; }
    public DateTime? DraftCompletedAtUtc { get; set; }
    public DateTime? TeamCompositionLockedAtUtc { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime Created { get; set; }
    public bool RequiresSubmission { get; set; }
    
    public ICollection<AssignmentFile> Files { get; set; } = new List<AssignmentFile>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    public ICollection<AssignmentCaptain> Captains { get; set; } = new List<AssignmentCaptain>();
    public ICollection<AssignmentTeam> Teams { get; set; } = new List<AssignmentTeam>();
}
