namespace PotteryClass.Data.Entities;

public class Comment
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public Guid UserId { get; set; }
    public string Text { get; set; }
    public DateTime Created { get; set; }
}