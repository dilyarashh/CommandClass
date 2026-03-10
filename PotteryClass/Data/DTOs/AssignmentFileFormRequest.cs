using Microsoft.AspNetCore.Http;
using PotteryClass.Data.Entities.Enums;

namespace PotteryClass.Data.DTOs;

public class AssignmentFileFormRequest
{
    public IFormFile File { get; set; } = null!;
}