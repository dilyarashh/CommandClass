namespace PotteryClass.Data.Entities;

public class AssignmentCaptain
{
    public Guid AssignmentId { get; set; }
    public Assignment Assignment { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }
}
