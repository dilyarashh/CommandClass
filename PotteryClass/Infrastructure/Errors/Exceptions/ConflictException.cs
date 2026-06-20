namespace PotteryClass.Infrastructure.Errors.Exceptions;

public class ConflictException(
    string code,
    string message,
    Dictionary<string, object>? details = null)
    : ApiException(409, code, message, details);
