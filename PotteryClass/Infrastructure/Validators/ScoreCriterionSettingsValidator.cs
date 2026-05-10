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

        RuleFor(x => x.SelectedValue)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x)
            .Must(x => x.MaxValue > x.MinValue)
            .WithMessage("РќРµРєРѕСЂСЂРµРєС‚РЅС‹Рµ РґРёР°РїР°Р·РѕРЅС‹");

        RuleFor(x => x)
            .Must(x => x.SelectedValue >= x.MinValue && x.SelectedValue <= x.MaxValue)
            .WithMessage("РќРµРєРѕСЂСЂРµРєС‚РЅС‹Рµ РґРёР°РїР°Р·РѕРЅС‹");

        RuleFor(x => x.Multiplier)
            .GreaterThanOrEqualTo(0)
            .When(x => x.Multiplier.HasValue)
            .WithMessage("РћС‚СЂРёС†Р°С‚РµР»СЊРЅС‹Рµ РјРЅРѕР¶РёС‚РµР»Рё РЅРµРґРѕРїСѓСЃС‚РёРјС‹");
    }
}
