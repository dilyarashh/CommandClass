namespace PotteryClass.Data.DTOs;

public class UpdateCourseRequest
{
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
}