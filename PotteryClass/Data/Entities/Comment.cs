namespace PotteryClass.Data.Entities;

public class Comment
{
    public Guid Id { get; set; }

    public Guid AssignmentId { get; set; }
    public Assignment Assignment { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Text { get; set; } = null!;

    public DateTime Created { get; set; }
}