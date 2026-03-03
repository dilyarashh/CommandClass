using PotteryClass.Data.Entities.Enums;

namespace PotteryClass.Data.DTOs;

public class UsersQuery
{
    public string? Search { get; set; }
    public UserRole? Role { get; set; }

    public string? SortBy { get; set; } = "Created";
    public bool Desc { get; set; } = true;

    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}