namespace PotteryClass.Data.DTOs;

public class MyCourseDto
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    public string? Description { get; set; }

    public string Code { get; set; } = default!;

    public bool IsActive { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public int TeacherCount { get; set; }
    public int StudentCount { get; set; }
    public CourseRegistrationDto Registration { get; set; } = new();

    public string Role { get; set; } = default!;

    public CourseAccessContextDto CurrentUser { get; set; } = new();

    public CoursePermissionsDto Permissions { get; set; } = new();
}
