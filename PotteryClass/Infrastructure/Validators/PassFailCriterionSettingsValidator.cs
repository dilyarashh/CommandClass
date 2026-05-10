using FluentValidation;
using PotteryClass.Data.DTOs;

namespace PotteryClass.Infrastructure.Validators;

public class PassFailCriterionSettingsValidator : AbstractValidator<PassFailCriterionSettingsDto>
{
    public PassFailCriterionSettingsValidator()
    {
        RuleFor(x => x.Multiplier)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Multiplier.HasValue)
            .WithMessage("Отрицательные множители недопустимы");

        RuleFor(x => x.Options)
            .NotNull()
            .Must(x => x.Count == 2)
            .WithMessage("Некорректные варианты");

        RuleForEach(x => x.Options)
            .ChildRules(option =>
            {
                option.RuleFor(x => x.Value)
                    .NotEmpty()
                    .MaximumLength(64);

                option.RuleFor(x => x.Label)
                    .MaximumLength(200);
            });

        RuleFor(x => x.ScoreMappings)
            .NotNull()
            .Must(x => x.Count == 2)
            .WithMessage("Invalid score mapping");
    }
}
