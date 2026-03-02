namespace PotteryClass.Infrastructure.Auth;

public class BlacklistTokenMiddleware(RequestDelegate next)
{
    public async Task Invoke(HttpContext context, AuthService authService)
    {
        var token = context.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        if (!string.IsNullOrEmpty(token) && await authService.IsTokenBlacklistedAsync(token))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Токен находится в BlackList");
            return;
        }

        await next(context);
    }
}
