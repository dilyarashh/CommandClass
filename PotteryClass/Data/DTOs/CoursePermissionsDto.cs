namespace PotteryClass.Data.DTOs;

public class CoursePermissionsDto
{
    public bool CanView { get; init; }
    public bool CanJoin { get; init; }
    public bool CanLeave { get; init; }
    public bool CanEdit { get; init; }
    public bool CanArchive { get; init; }
    public bool CanRestore { get; init; }
    public bool CanManageTeachers { get; init; }
    public bool CanManageStudents { get; init; }
    public bool CanCreateAssignments { get; init; }
    public bool CanManageAssignments { get; init; }
    public bool CanManageTeams { get; init; }
    public bool CanGradeSubmissions { get; init; }
}
