using PotteryClass.Data.Entities.Enums;

namespace PotteryClass.Data.DTOs;

public class UserDto
{
    public required Guid Id { get; set; }
    public required string FirstName { get; set; }
    public required  string LastName { get; set; }
    public required  string MiddleName { get; set; }
    public required string Email { get; set; } 
    public UserRole Role { get; set; }
    public UserRole EffectiveRole { get; set; }
    public UserPermissionsDto Permissions { get; set; } = new();
}
