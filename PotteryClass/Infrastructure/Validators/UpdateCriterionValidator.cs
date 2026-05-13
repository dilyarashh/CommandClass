using FluentValidation;
using PotteryClass.Data.DTOs;

namespace PotteryClass.Infrastructure.Validators;

public class UpdateCriterionValidator : AbstractValidator<UpdateCriterionRequest>
{
    public UpdateCriterionValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200)
            .When(x => x.Name != null);

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => x.Description != null);

        RuleFor(x => x.Type)
            .NotEmpty()
            .MaximumLength(64)
            .When(x => x.Type != null);

        RuleFor(x => x.Category)
            .NotEmpty()
            .MaximumLength(64)
            .When(x => x.Category != null);

        RuleFor(x => x.MaxScore)
            .GreaterThan(0)
            .When(x => x.MaxScore.HasValue);

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0)
            .When(x => x.SortOrder.HasValue);
    }
}
