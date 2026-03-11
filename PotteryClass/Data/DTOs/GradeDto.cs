namespace PotteryClass.Data.DTOs;

public class GradeDto
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public Guid StudentId { get; set; }
    public Guid TeacherId { get; set; }
    public int Value { get; set; }
}