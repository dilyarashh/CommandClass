namespace PotteryClass.Data.DTOs;

public class UserPermissionsDto
{
    public bool CanCreateCourses { get; init; }
    public bool CanManageUsers { get; init; }
    public bool CanAssignTeachers { get; init; }
    public bool CanManageSystem { get; init; }
}
