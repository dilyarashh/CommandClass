namespace PotteryClass.Data.Entities;

public class CriterionGroup
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreatedAtUtc { get; set; }

    public Assignment Assignment { get; set; } = null!;
}
