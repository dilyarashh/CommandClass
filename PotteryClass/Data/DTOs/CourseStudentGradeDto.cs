namespace PotteryClass.Data.DTOs;

public class CourseStudentGradeDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; } = null!;

    public Guid AssignmentId { get; set; }
    public string AssignmentTitle { get; set; } = null!;

    public int? Grade { get; set; }
}