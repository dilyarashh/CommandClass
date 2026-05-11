using FluentValidation;
using PotteryClass.Data.DTOs;

namespace PotteryClass.Infrastructure.Validators;

public class AssignmentGradingRulesValidator : AbstractValidator<AssignmentGradingRulesDto>
{
    public AssignmentGradingRulesValidator()
    {
        RuleFor(x => x.Mode)
            .NotEmpty()
            .Must(mode =>
            {
                if (string.IsNullOrWhiteSpace(mode))
                    return false;

                var normalizedMode = mode.Trim().ToLowerInvariant();
                return normalizedMode == AssignmentGradingModeDto.SumPoints ||
                       normalizedMode == AssignmentGradingModeDto.BaseWithMultipliers;
            })
            .WithMessage("Unsupported grading mode");

        RuleFor(x => x.BaseGrade)
            .GreaterThan(0)
            .When(x => x.BaseGrade.HasValue);

        RuleFor(x => x.MainCriteriaThreshold)
            .NotNull();

        RuleFor(x => x.Penalties)
            .NotNull();

        RuleFor(x => x.MainCriteriaThreshold)
            .SetValidator(new MainCriteriaThresholdSettingsValidator());

        RuleFor(x => x.Penalties)
            .SetValidator(new AssignmentPenaltySettingsValidator());
    }
}

public class MainCriteriaThresholdSettingsValidator : AbstractValidator<MainCriteriaThresholdSettingsDto>
{
    public MainCriteriaThresholdSettingsValidator()
    {
        RuleFor(x => x.Threshold)
            .InclusiveBetween(0, 100)
            .When(x => x.Threshold.HasValue);

        RuleFor(x => x.Behavior)
            .Must(behavior =>
            {
                if (string.IsNullOrWhiteSpace(behavior))
                    return false;

                var normalizedBehavior = behavior.Trim().ToLowerInvariant();
                return normalizedBehavior == MainCriteriaThresholdBehaviorDto.SetToZero ||
                       normalizedBehavior == MainCriteriaThresholdBehaviorDto.MarkAsFailed;
            })
            .When(x => !string.IsNullOrWhiteSpace(x.Behavior))
            .WithMessage("Unsupported threshold behavior");
    }
}

public class AssignmentPenaltySettingsValidator : AbstractValidator<AssignmentPenaltySettingsDto>
{
    public AssignmentPenaltySettingsValidator()
    {
        RuleFor(x => x.Deadline)
            .NotNull()
            .SetValidator(new PenaltyRuleValidator());

        RuleFor(x => x.Progress)
            .NotNull()
            .SetValidator(new PenaltyRuleValidator());

        RuleFor(x => x.RequiredCriteria)
            .NotNull()
            .SetValidator(new PenaltyRuleValidator());
    }
}

public class PenaltyRuleValidator : AbstractValidator<PenaltyRuleDto>
{
    public PenaltyRuleValidator()
    {
        RuleFor(x => x.Percentage)
            .InclusiveBetween(0, 100)
            .When(x => x.Percentage.HasValue);
    }
}
