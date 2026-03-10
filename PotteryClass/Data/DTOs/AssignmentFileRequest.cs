using PotteryClass.Data.Entities.Enums;

namespace PotteryClass.Data.DTOs;

public class AssignmentFileRequest
{
    public string FileName { get; set; } = null!;
    public byte[] Content { get; set; } = null!;
    public string MimeType { get; set; } = null!;
    public FileType Type { get; set; }
}