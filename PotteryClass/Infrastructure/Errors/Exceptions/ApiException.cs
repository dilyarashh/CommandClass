namespace PotteryClass.Infrastructure.Errors.Exceptions;

public class ApiException(
    int status,
    string code,
    string message,
    Dictionary<string, object>? details = null)
    : Exception(message)
{
    public int Status { get; } = status;
    public string Code { get; } = code;
    public Dictionary<string, object>? Details { get; } = details;
}
