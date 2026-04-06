namespace PotteryClass.Data.Entities;

public class AssignmentTeamMember
{
    public Guid TeamId { get; set; }
    public AssignmentTeam Team { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }
}
