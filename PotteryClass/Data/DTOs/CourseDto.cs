namespace PotteryClass.Data.DTOs
{
    public class CourseDto
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = null!;
        public string? Description { get; init; }
        public string Code { get; init; } = null!;
        public bool IsActive { get; init; }
        public DateTime RegistrationStartsAtUtc { get; init; }
        public DateTime RegistrationEndsAtUtc { get; init; }
    }
}
