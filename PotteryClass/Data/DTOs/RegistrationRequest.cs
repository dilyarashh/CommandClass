using PotteryClass.Data.Entities.Enums;

namespace PotteryClass.Data.DTOs;

public class RegistrationRequest
{ 
    public required string FirstName { get; set; }
    public required  string LastName { get; set; }
    public required  string MiddleName { get; set; }
    public required  string Email { get; set; } 
    public required string Password { get; set; }
}