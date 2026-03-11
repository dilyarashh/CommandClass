using Microsoft.AspNetCore.Http;

namespace PotteryClass.Data.DTOs;

public class SubmissionFileFormRequest
{
    public IFormFile File { get; set; } = null!;
}