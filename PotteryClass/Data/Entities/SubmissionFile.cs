using PotteryClass.Data.Entities;
using PotteryClass.Data.Entities.Enums;

public class SubmissionFile
{
    public Guid Id { get; set; }
    public Guid SubmissionId { get; set; }
    public Submission Submission { get; set; } = null!;
    public string FileName { get; set; }
    public string Url { get; set; }
    public string MimeType { get; set; }
    public long Size { get; set; }
    public FileType Type { get; set; }
}
