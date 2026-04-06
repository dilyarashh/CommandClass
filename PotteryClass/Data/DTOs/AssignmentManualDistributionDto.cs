namespace PotteryClass.Data.DTOs;

public class AssignmentManualDistributionDto
{
    public List<AssignmentTeamDto> Teams { get; set; } = new();
    public List<CourseStudentDto> AvailableStudents { get; set; } = new();
}
