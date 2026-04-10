namespace PotteryClass.Data.DTOs;

public class CaptainAssignmentContextDto
{
    public Guid AssignmentId { get; set; }
    public bool IsCaptain { get; set; }
    public Guid? TeamId { get; set; }
    public Guid? FinalSubmissionId { get; set; }
    public bool CanSelectFinalSubmission { get; set; }
}
