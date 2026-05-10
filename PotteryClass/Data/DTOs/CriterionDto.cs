namespace PotteryClass.Data.DTOs;

public class CriterionDto
{
    public Guid Id { get; set; }
    public Guid CriterionGroupId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string Type { get; set; } = null!;
    public string Settings { get; set; } = null!;
    public int MaxScore { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
