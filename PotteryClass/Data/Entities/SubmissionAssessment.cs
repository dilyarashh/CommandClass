namespace PotteryClass.Data.Entities;

public class SubmissionAssessment
{
    public Guid Id { get; set; }
    public Guid SubmissionId { get; set; }
    public Guid AssignmentId { get; set; }
    public Guid StudentId { get; set; }
    public Guid CheckedByUserId { get; set; }
    public string CriterionValues { get; set; } = null!;
    public decimal MainPoints { get; set; }
    public decimal BonusPoints { get; set; }
    public decimal PenaltyPoints { get; set; }
    public decimal Multiplier { get; set; }
    public decimal FinalGrade { get; set; }
    public string CalculationDetails { get; set; } = null!;
    public DateTime CheckedAtUtc { get; set; }
    public string? Comment { get; set; }

    public Submission Submission { get; set; } = null!;
}
