namespace PotteryClass.Data.DTOs;

public class AssignmentTeamDto
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public string Name { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; }
    public List<AssignmentTeamMemberDto> Members { get; set; } = new();
}
