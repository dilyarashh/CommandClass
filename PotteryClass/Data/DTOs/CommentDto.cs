namespace PotteryClass.Data.DTOs;

public class CommentDto
{
	public Guid Id { get; init; }

    public Guid AssignmentId { get; set; }

    public Guid UserId { get; init; }

    public string UserName { get; set; } = null!;

    public string Text { get; init; } = null!;

	public DateTime Created { get; init; }
}