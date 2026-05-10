using FluentValidation;
using PotteryClass.Data.DTOs;

namespace PotteryClass.Infrastructure.Validators;

public class ScoreCriterionSettingsValidator : AbstractValidator<ScoreCriterionSettingsDto>
{
    public ScoreCriterionSettingsValidator()
    {
        RuleFor(x => x.MinValue)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.MaxValue)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x)
            .Must(x => x.MaxValue >= x.MinValue)
            .WithMessage("Некорректные диапазоны");

        RuleFor(x => x.Multiplier)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Multiplier.HasValue)
            .WithMessage("Отрицательные множители недопустимы");
    }
}
