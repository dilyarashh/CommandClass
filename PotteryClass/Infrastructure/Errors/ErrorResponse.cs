using System.Text.Json.Serialization;

namespace PotteryClass.Infrastructure.Errors;

public class ErrorResponse
{
    public string Title { get; set; } = null!;
    public int Status { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Code { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Message { get; set; }
    public string? Detail { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Dictionary<string, object>? Details { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
}
