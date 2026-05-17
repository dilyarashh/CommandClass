using FluentValidation;
using PotteryClass.Data.DTOs;

namespace PotteryClass.Infrastructure.Validators;

public class MultiplierCriterionSettingsValidator : AbstractValidator<MultiplierCriterionSettingsDto>
{
    public MultiplierCriterionSettingsValidator()
    {
        RuleFor(x => x.Coefficient)
            .GreaterThanOrEqualTo(0)
            .WithMessage("РћС‚СЂРёС†Р°С‚РµР»СЊРЅС‹Рµ РјРЅРѕР¶РёС‚РµР»Рё РЅРµРґРѕРїСѓСЃС‚РёРјС‹");
    }
}
