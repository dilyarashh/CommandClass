using FluentValidation;
using PotteryClass.Data.DTOs;

namespace PotteryClass.Infrastructure.Validators;

public class CreateCriterionValidator : AbstractValidator<CreateCriterionRequest>
{
    public CreateCriterionValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(2000);

        RuleFor(x => x.Type)
            .NotEmpty()
            .MaximumLength(64);

        RuleFor(x => x.Category)
            .MaximumLength(64)
            .When(x => x.Category != null);

        RuleFor(x => x.MaxScore)
            .GreaterThan(0);

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0);
    }
}
