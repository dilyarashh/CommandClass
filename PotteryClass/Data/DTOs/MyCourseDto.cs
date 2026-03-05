namespace PotteryClass.Data.DTOs;

public class MyCourseDto
{
	public Guid Id { get; set; }

	public string Name { get; set; } = default!;

	public string Code { get; set; } = default!;

	public string Role { get; set; } = default!;
}