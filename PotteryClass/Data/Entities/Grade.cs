namespace PotteryClass.Data.Entities;

public class Grade
{
    public Guid Id { get; set; }

    public Guid AssignmentId { get; set; }
    public Assignment Assignment { get; set; } = null!;

    public Guid StudentId { get; set; }
    public User Student { get; set; } = null!;

    public Guid TeacherId { get; set; }
    public User Teacher { get; set; } = null!;

    public int Value { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}