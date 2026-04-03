namespace PotteryClass.Data.DTOs
{
    public class CreateCourseRequest
    {
        public string Name { get; init; } = null!;
        public string? Description { get; init; }
    }
}
