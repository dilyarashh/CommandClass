namespace PotteryClass.Data.DTOs;

public class CreateCriterionGroupRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
}
