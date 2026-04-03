using PotteryClass.Data.DTOs;
using PotteryClass.Data.Entities;
using PotteryClass.Data.Entities.Enums;
using PotteryClass.Infrastructure.Auth;

namespace PotteryClass.Services;

public class AccessContextService(ICurrentUser currentUser) : IAccessContextService
{
    private static CourseRegistrationDto BuildRegistration(Course course)
    {
        var now = DateTime.UtcNow;
        var status = now < course.RegistrationStartsAtUtc
            ? "Upcoming"
            : now <= course.RegistrationEndsAtUtc
                ? "Open"
                : "Closed";

        return new CourseRegistrationDto
        {
            OpensAtUtc = course.RegistrationStartsAtUtc,
            ClosesAtUtc = course.RegistrationEndsAtUtc,
            Status = status
        };
    }

    public CourseAccessContextDto BuildCourseAccessContext(Course course)
    {
        var globalRole = currentUser.GetRole();
        var userId = currentUser.GetUserId();

        var studentLink = course.Students.FirstOrDefault(x => x.UserId == userId);
        var isAdmin = globalRole == UserRole.Admin;
        var isTeacher = isAdmin || course.Teachers.Any(x => x.UserId == userId);
        var isStudent = studentLink != null;
        var isBlocked = studentLink?.IsBlocked ?? false;

        return new CourseAccessContextDto
        {
            GlobalRole = globalRole,
            EffectiveRole = isAdmin
                ? UserRole.Admin
                : isTeacher
                    ? UserRole.Teacher
                    : UserRole.Student,
            CourseRole = isAdmin
                ? UserRole.Admin.ToString()
                : isTeacher
                    ? UserRole.Teacher.ToString()
                    : isStudent
                        ? UserRole.Student.ToString()
                        : "None",
            IsMember = isAdmin || isTeacher || isStudent,
            IsTeacher = isTeacher,
            IsStudent = isStudent,
            IsBlocked = isBlocked
        };
    }

    public CoursePermissionsDto BuildCoursePermissions(Course course)
    {
        var access = BuildCourseAccessContext(course);
        var canView = access.IsTeacher || (access.IsStudent && !access.IsBlocked);
        var now = DateTime.UtcNow;
        var registrationOpen = course.IsActive &&
                               now >= course.RegistrationStartsAtUtc &&
                               now <= course.RegistrationEndsAtUtc;

        return new CoursePermissionsDto
        {
            CanView = canView,
            CanJoin = !access.IsTeacher && !access.IsStudent && registrationOpen,
            CanLeave = access.IsStudent || (access.IsTeacher && course.CreatedByUserId != currentUser.GetUserId()),
            CanEdit = access.IsTeacher,
            CanArchive = access.GlobalRole == UserRole.Admin && course.IsActive,
            CanRestore = access.GlobalRole == UserRole.Admin && !course.IsActive,
            CanManageTeachers = access.GlobalRole == UserRole.Admin,
            CanManageStudents = access.IsTeacher,
            CanCreateAssignments = access.IsTeacher,
            CanManageAssignments = access.IsTeacher,
            CanManageTeams = access.IsTeacher,
            CanGradeSubmissions = access.IsTeacher
        };
    }

    public AssignmentPermissionsDto BuildAssignmentPermissions(Course course)
    {
        var access = BuildCourseAccessContext(course);
        var canView = access.IsTeacher || (access.IsStudent && !access.IsBlocked);

        return new AssignmentPermissionsDto
        {
            CanView = canView,
            CanEdit = access.IsTeacher,
            CanDelete = access.IsTeacher,
            CanUploadFiles = access.IsTeacher,
            CanDeleteFiles = access.IsTeacher,
            CanViewSubmissions = access.IsTeacher,
            CanSubmit = access.IsStudent && !access.IsBlocked,
            CanGradeSubmissions = access.IsTeacher
        };
    }

    public UserRole GetEffectiveRole(UserRole globalRole, bool teachesAnywhere)
    {
        if (globalRole == UserRole.Admin)
        {
            return UserRole.Admin;
        }

        return teachesAnywhere ? UserRole.Teacher : UserRole.Student;
    }

    public UserPermissionsDto BuildUserPermissions(UserRole effectiveRole)
    {
        var isAdmin = effectiveRole == UserRole.Admin;
        var canCreateCourses = isAdmin || effectiveRole == UserRole.Teacher;

        return new UserPermissionsDto
        {
            CanCreateCourses = canCreateCourses,
            CanManageUsers = isAdmin,
            CanAssignTeachers = isAdmin,
            CanManageSystem = isAdmin
        };
    }

}
