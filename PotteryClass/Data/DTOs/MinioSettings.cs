namespace PotteryClass.Data.DTOs;

public class MinioSettings
{
    public string Endpoint { get; set; } = null!;
    public string AccessKey { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public string Bucket { get; set; } = null!;
    public bool UseSsl { get; set; } = true;
    public string? PublicBaseUrl { get; set; }
    public string? ObjectPrefix { get; set; }
}