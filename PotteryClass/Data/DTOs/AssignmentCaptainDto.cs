namespace PotteryClass.Data.DTOs;

public class AssignmentCaptainDto
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public DateTime CreatedAtUtc { get; set; }
}
