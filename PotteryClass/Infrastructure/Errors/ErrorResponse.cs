namespace PotteryClass.Infrastructure.Errors;

public class ErrorResponse
{
    public string Title { get; set; } = null!;
    public int Status { get; set; }
    public string? Detail { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
}
