namespace PotteryClass.Data.DTOs;

public static class AssignmentGradingModeDto
{
    public const string SumPoints = "sum_points";
    public const string BaseWithMultipliers = "base_with_multipliers";
}

public static class MainCriteriaThresholdBehaviorDto
{
    public const string SetToZero = "set_to_zero";
    public const string MarkAsFailed = "mark_as_failed";
}

public class AssignmentGradingRulesDto
{
    public string Mode { get; set; } = AssignmentGradingModeDto.SumPoints;
    public decimal? BaseGrade { get; set; }
    public MainCriteriaThresholdSettingsDto MainCriteriaThreshold { get; set; } = new();
    public AssignmentPenaltySettingsDto Penalties { get; set; } = new();
}

public class MainCriteriaThresholdSettingsDto
{
    public bool Enabled { get; set; }
    public decimal? Threshold { get; set; }
    public string? Behavior { get; set; }
}

public class AssignmentPenaltySettingsDto
{
    public PenaltyRuleDto Deadline { get; set; } = new();
    public PenaltyRuleDto Progress { get; set; } = new();
    public PenaltyRuleDto RequiredCriteria { get; set; } = new();
}

public class PenaltyRuleDto
{
    public bool Enabled { get; set; }
    public decimal? Percentage { get; set; }
}
