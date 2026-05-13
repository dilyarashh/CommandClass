using System.Text.Json;

namespace PotteryClass.Data.DTOs;

public class GradeCalculationRequest
{
    public AssignmentGradingRulesDto Rules { get; set; } = new();
    public List<CriterionDto> Criteria { get; set; } = new();
    public List<CriterionValueDto> Values { get; set; } = new();
    public GradePenaltyInputDto Penalties { get; set; } = new();
}

public class CriterionValueDto
{
    public Guid CriterionId { get; set; }
    public JsonElement Value { get; set; }
}

public class GradePenaltyInputDto
{
    public bool Deadline { get; set; }
    public bool Progress { get; set; }
    public bool RequiredCriteria { get; set; }
}

public class GradeCalculationResultDto
{
    public decimal MainPoints { get; set; }
    public decimal BonusPoints { get; set; }
    public decimal PenaltyPoints { get; set; }
    public decimal Multiplier { get; set; } = 1m;
    public decimal FinalGrade { get; set; }
    public bool MainCriteriaThresholdPassed { get; set; } = true;
    public bool IsFailed { get; set; }
    public List<CriterionCalculationDetailDto> Criteria { get; set; } = new();
    public List<AppliedPenaltyDetailDto> AppliedPenalties { get; set; } = new();
    public List<AppliedMultiplierDetailDto> AppliedMultipliers { get; set; } = new();
}

public class CriterionCalculationDetailDto
{
    public Guid CriterionId { get; set; }
    public string Name { get; set; } = null!;
    public string Type { get; set; } = null!;
    public string Category { get; set; } = null!;
    public decimal Score { get; set; }
    public decimal? Multiplier { get; set; }
    public bool IsApplied { get; set; }
}

public class AppliedPenaltyDetailDto
{
    public string Source { get; set; } = null!;
    public decimal Value { get; set; }
    public string Kind { get; set; } = "points";
}

public class AppliedMultiplierDetailDto
{
    public string Source { get; set; } = null!;
    public decimal Value { get; set; }
}
