namespace PotteryClass.Data.DTOs;

public class SubmissionAssessmentFormDto
{
    public Guid SubmissionId { get; set; }
    public Guid AssignmentId { get; set; }
    public Guid StudentId { get; set; }
    public AssignmentGradingRulesDto Rules { get; set; } = new();
    public List<SubmissionAssessmentCriterionGroupDto> Groups { get; set; } = new();
    public SubmissionAssessmentDto? SavedAssessment { get; set; }
}

public class SubmissionAssessmentCriterionGroupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int SortOrder { get; set; }
    public List<CriterionDto> Criteria { get; set; } = new();
}
