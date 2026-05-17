using System.Text.Json;

namespace PotteryClass.Data.DTOs;

public class CreateCriterionRequest
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public string Type { get; set; } = null!;
    public string? Category { get; set; }
    public JsonElement? Settings { get; set; }
    public int MaxScore { get; set; }
    public int SortOrder { get; set; }
}
