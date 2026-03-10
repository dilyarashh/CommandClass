namespace PotteryClass.Data.DTOs;

public class PagedAssignmentResult<T>
{
    public List<T> Items { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}