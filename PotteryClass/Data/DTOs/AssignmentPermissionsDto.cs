namespace PotteryClass.Data.DTOs;

public class AssignmentPermissionsDto
{
    public bool CanView { get; init; }
    public bool CanEdit { get; init; }
    public bool CanDelete { get; init; }
    public bool CanUploadFiles { get; init; }
    public bool CanDeleteFiles { get; init; }
    public bool CanViewSubmissions { get; init; }
    public bool CanSubmit { get; init; }
    public bool CanGradeSubmissions { get; init; }
}
