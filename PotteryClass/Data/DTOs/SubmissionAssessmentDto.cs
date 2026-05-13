using System.Text.Json;

namespace PotteryClass.Data.DTOs;

public class SaveSubmissionAssessmentRequest
{
    public List<CriterionValueDto> Values { get; set; } = new();
    public GradePenaltyInputDto Penalties { get; set; } = new();
    public string? Comment { get; set; }
}

public class SubmissionAssessmentDto
{
    public Guid Id { get; set; }
    public Guid SubmissionId { get; set; }
    public Guid AssignmentId { get; set; }
    public Guid StudentId { get; set; }
    public Guid CheckedByUserId { get; set; }
    public JsonElement CriterionValues { get; set; }
    public decimal MainPoints { get; set; }
    public decimal BonusPoints { get; set; }
    public decimal PenaltyPoints { get; set; }
    public decimal Multiplier { get; set; }
    public decimal FinalGrade { get; set; }
    public JsonElement CalculationDetails { get; set; }
    public DateTime CheckedAtUtc { get; set; }
    public string? Comment { get; set; }
}
