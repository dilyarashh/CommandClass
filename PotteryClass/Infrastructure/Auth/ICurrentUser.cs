using PotteryClass.Data.Entities.Enums;

namespace PotteryClass.Infrastructure.Auth;

public interface ICurrentUser
{
    Guid GetUserId();
    UserRole GetRole();
    bool IsAuthenticated();
}
