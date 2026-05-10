using FluentValidation;
using PotteryClass.Data.DTOs;

namespace PotteryClass.Infrastructure.Validators;

public class CreateCriterionGroupValidator : AbstractValidator<CreateCriterionGroupRequest>
{
    public CreateCriterionGroupValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(2000);

        RuleFor(x => x.SortOrder)
            .GreaterThanOrEqualTo(0);
    }
}
