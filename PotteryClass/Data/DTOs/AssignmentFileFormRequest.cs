using PotteryClass.Data.Entities.Enums;

namespace PotteryClass.Data.DTOs;

public class AssignmentFileFormRequest
{
    public string FileName { get; set; } = null!;
    public IFormFile Content { get; set; } = null!;
    public FileType Type { get; set; }
}