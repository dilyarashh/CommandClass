namespace PotteryClass.Data.DTOs;

public class CourseRegistrationDto
{
    public DateTime OpensAtUtc { get; init; }
    public DateTime ClosesAtUtc { get; init; }
    public string Status { get; init; } = "Closed";
}
