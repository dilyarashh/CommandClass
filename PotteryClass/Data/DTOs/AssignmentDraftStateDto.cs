namespace PotteryClass.Data.DTOs;

public class AssignmentDraftStateDto
{
    public bool IsStarted { get; set; }
    public bool IsCompleted { get; set; }
    public Guid? CurrentCaptainUserId { get; set; }
    public DateTime? StartedAtUtc { get; set; }
    public DateTime? CompletedAtUtc { get; set; }
    public List<AssignmentTeamDto> Teams { get; set; } = new();
    public List<CourseStudentDto> AvailableStudents { get; set; } = new();
}
