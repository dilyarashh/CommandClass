namespace PotteryClass.Data.Entities;

public class BlackToken
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Token { get; set; }
    public DateTime ExpiredAt { get; set; } = DateTime.UtcNow;
}