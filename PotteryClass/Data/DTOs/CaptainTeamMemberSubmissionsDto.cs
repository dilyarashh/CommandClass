namespace PotteryClass.Data.DTOs;

public class CaptainTeamMemberSubmissionsDto
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public List<SubmissionDto> Submissions { get; set; } = new();
}
