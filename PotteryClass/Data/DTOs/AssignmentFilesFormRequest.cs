using System.Collections.Generic;

namespace PotteryClass.Data.DTOs;

public class AssignmentFilesFormRequest
{
    public List<AssignmentFileFormRequest> Files { get; set; } = new();
}