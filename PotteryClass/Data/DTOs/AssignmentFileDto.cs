using PotteryClass.Data.Entities.Enums;

namespace PotteryClass.Data.DTOs;

public class AssignmentFileDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string MimeType { get; set; } = null!;
    public long Size { get; set; }
    public FileType Type { get; set; }
}