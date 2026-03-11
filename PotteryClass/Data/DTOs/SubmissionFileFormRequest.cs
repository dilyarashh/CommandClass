using PotteryClass.Data.Entities.Enums;

namespace PotteryClass.Data.DTOs;

public class SubmissionFileFormRequest
{
    public IFormFile File { get; set; } = null!;
    public FileType Type { get; set; }
}