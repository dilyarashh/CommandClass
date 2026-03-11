using PotteryClass.Data.Entities.Enums;

namespace PotteryClass.Data.DTOs;

public class SubmissionFileDto
{
    public Guid Id { get; set; }
    public string FileName { get; set; }
    public string Url { get; set; }
    public string MimeType { get; set; }
    public long Size { get; set; }
    public FileType Type { get; set; }
}