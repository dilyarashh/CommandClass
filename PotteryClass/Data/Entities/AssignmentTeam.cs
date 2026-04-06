namespace PotteryClass.Data.Entities;

public class AssignmentTeam
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public Assignment Assignment { get; set; } = null!;
    public string Name { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; }

    public ICollection<AssignmentTeamMember> Members { get; set; } = new List<AssignmentTeamMember>();
}
