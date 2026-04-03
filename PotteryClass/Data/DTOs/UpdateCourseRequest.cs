namespace PotteryClass.Data.DTOs;

public class UpdateCourseRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public DateTime? RegistrationStartsAtUtc { get; init; }
    public DateTime? RegistrationEndsAtUtc { get; init; }
}
