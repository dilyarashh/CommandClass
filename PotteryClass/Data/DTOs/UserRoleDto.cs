using PotteryClass.Data.Entities.Enums;

namespace PotteryClass.Data.DTOs;

public class UserRoleDto
{
    public UserRole GlobalRole { get; set; }
    public UserRole EffectiveRole { get; set; }
    public UserPermissionsDto Permissions { get; set; } = new();
}
