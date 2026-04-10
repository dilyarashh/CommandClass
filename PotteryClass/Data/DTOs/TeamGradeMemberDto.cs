namespace PotteryClass.Data.DTOs;

public class TeamGradeMemberDto
{
    public Guid StudentId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string? MiddleName { get; set; }
    public Guid? SubmissionId { get; set; }
    public int? Grade { get; set; }
    public string? TeacherComment { get; set; }
}
