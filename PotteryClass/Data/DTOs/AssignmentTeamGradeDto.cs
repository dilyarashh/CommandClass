namespace PotteryClass.Data.DTOs;

public class AssignmentTeamGradeDto
{
    public Guid TeamId { get; set; }
    public Guid AssignmentId { get; set; }
    public string TeamName { get; set; } = null!;
    public Guid? FinalSubmissionId { get; set; }
    public decimal? TeamGrade { get; set; }
    public SubmissionDto? FinalSubmission { get; set; }
    public List<TeamGradeMemberDto> Members { get; set; } = new();
}
