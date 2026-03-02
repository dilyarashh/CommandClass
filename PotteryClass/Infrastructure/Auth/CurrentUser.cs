using System.Security.Claims;
using PotteryClass.Data.Entities.Enums;
using PotteryClass.Infrastructure.Errors.Exceptions;

namespace PotteryClass.Infrastructure.Auth;

public class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal User =>
        httpContextAccessor.HttpContext?.User
        ?? throw new UnauthorizedException("Пользователь не авторизован");

    public Guid GetUserId()
    {
        var id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(id))
            throw new UnauthorizedException("ID пользователя не найден");

        return Guid.Parse(id);
    }
    
    public UserRole GetRole()
    {
        var role = User.FindFirst(ClaimTypes.Role)?.Value;

        if (string.IsNullOrEmpty(role))
            throw new UnauthorizedException("Роль пользователя не найдена");

        return Enum.Parse<UserRole>(role);
    }

    public bool IsAuthenticated()
        => User.Identity?.IsAuthenticated ?? false;
}
