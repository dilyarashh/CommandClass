namespace PotteryClass.Data.DTOs;

public class AssignmentDto
{
    public Guid Id { get; set; }

    public Guid CourseId { get; set; }

    public string Title { get; set; }

    public string Text { get; set; }

    public string Status { get; set; } = AssignmentStatus.Hidden;

    public DateTime? StartsAtUtc { get; set; }

    public int? MinTeamSize { get; set; }

    public int? MaxTeamSize { get; set; }

    public string TeamFormationMode { get; set; } = AssignmentTeamFormationModeDto.TeacherManaged;

    public DateTime? CaptainSelectionEndsAtUtc { get; set; }

    public DateTime? TeamFormationStartsAtUtc { get; set; }

    public DateTime? TeamFormationEndsAtUtc { get; set; }

    public Guid? DraftCurrentCaptainUserId { get; set; }

    public DateTime? DraftStartedAtUtc { get; set; }

    public DateTime? DraftCompletedAtUtc { get; set; }

    public bool IsTeamCompositionLocked { get; set; }

    public DateTime? TeamCompositionLockedAtUtc { get; set; }

    public bool RequiresSubmission { get; set; }

    public DateTime? Deadline { get; set; }

    public DateTime Created { get; set; }
    
    public List<AssignmentFileDto> Files { get; set; } = new();
}
