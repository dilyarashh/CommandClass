namespace PotteryClass.Data.DTOs;

public class CaptainTeamDto
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public string Name { get; set; } = null!;
    public AssignmentCaptainDto Captain { get; set; } = null!;
    public Guid? FinalSubmissionId { get; set; }
    public List<CaptainTeamMemberSubmissionsDto> Members { get; set; } = new();
}
