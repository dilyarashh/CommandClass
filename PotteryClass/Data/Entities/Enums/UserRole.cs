using System.Text.Json.Serialization;

namespace PotteryClass.Data.Entities.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum UserRole
{
    Admin = 1,
    Teacher = 2,
    Student = 3
}