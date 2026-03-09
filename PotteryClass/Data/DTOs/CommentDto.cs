namespace PotteryClass.Data.DTOs;

public class CommentDto
{
	public Guid Id { get; init; }

	public Guid AssignmentId { get; init; }

	public Guid UserId { get; init; }

	public string Text { get; init; } = null!;

	public DateTime Created { get; init; }
}