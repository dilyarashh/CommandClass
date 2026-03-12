namespace PotteryClass.Data.DTOs;

public class MyCourseDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public string? Description { get; set; }

    public string Code { get; set; } = default!;

    public bool IsActive { get; set; }

    public string Role { get; set; } = default!;
}