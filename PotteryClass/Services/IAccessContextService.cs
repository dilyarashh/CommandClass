using PotteryClass.Data.DTOs;
using PotteryClass.Data.Entities;
using PotteryClass.Data.Entities.Enums;

namespace PotteryClass.Services;

public interface IAccessContextService
{
    CourseAccessContextDto BuildCourseAccessContext(Course course);
    CoursePermissionsDto BuildCoursePermissions(Course course);
    AssignmentPermissionsDto BuildAssignmentPermissions(Course course);
    UserRole GetEffectiveRole(UserRole globalRole, bool teachesAnywhere);
    UserPermissionsDto BuildUserPermissions(UserRole effectiveRole);
}
