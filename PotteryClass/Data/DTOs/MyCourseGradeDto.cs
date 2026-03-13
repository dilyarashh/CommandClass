namespace PotteryClass.Data.DTOs;

public class MyCourseGradeDto
{
    public Guid AssignmentId { get; set; }
    public string AssignmentTitle { get; set; } = null!;
    public int? Grade { get; set; }
}