using PotteryClass.Data.Entities.Enums;

namespace PotteryClass.Data.DTOs;

public class CourseAccessContextDto
{
    public UserRole GlobalRole { get; init; }
    public UserRole EffectiveRole { get; init; }
    public string CourseRole { get; init; } = "None";
    public bool IsMember { get; init; }
    public bool IsTeacher { get; init; }
    public bool IsStudent { get; init; }
    public bool IsBlocked { get; init; }
}
