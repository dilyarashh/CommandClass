using PotteryClass.Data.Entities.Enums;

namespace PotteryClass.Data.Entities;

public class AssignmentFile
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public Assignment Assignment { get; set; } = null!;
    public string FileName { get; set; }
    public string Url { get; set; }
    public string MimeType { get; set; }
    public long Size { get; set; }
    public FileType Type { get; set; }
}
