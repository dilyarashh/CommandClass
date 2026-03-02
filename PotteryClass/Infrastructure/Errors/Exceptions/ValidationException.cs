namespace PotteryClass.Infrastructure.Errors.Exceptions;

public class ValidationException(Dictionary<string, string[]> errors) : Exception("Validation failed")
{
    public Dictionary<string, string[]> Errors { get; } = errors;
}
