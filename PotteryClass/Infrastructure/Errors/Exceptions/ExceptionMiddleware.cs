namespace PotteryClass.Infrastructure.Errors.Exceptions;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = exception switch
        {
            NotFoundException => CreateResponse(404, exception.Message),
            ForbiddenException => CreateResponse(403, exception.Message),
            UnauthorizedException => CreateResponse(401, exception.Message),
            BadRequestException => CreateResponse(400, exception.Message),
            ValidationException validationEx => new ErrorResponse
            {
                Title = "Validation failed",
                Status = 400,
                Errors = validationEx.Errors
            },
            _ => CreateResponse(500, "Internal server error")
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = response.Status;

        return context.Response.WriteAsJsonAsync(response);
    }

    private static ErrorResponse CreateResponse(int status, string message)
        => new()
        {
            Title = message,
            Status = status
        };
}
