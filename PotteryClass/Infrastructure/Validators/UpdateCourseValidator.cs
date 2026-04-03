using FluentValidation;
using PotteryClass.Data.DTOs;

namespace PotteryClass.Infrastructure.Validators;

public class UpdateCourseValidator : AbstractValidator<UpdateCourseRequest>
{
    public UpdateCourseValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(200)
            .When(x => x.Name is not null);

        RuleFor(x => x.Name)
            .NotEmpty()
            .When(x => x.Name is not null);

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .When(x => x.Description is not null);

        RuleFor(x => x)
            .Must(x => !x.RegistrationStartsAtUtc.HasValue || !x.RegistrationEndsAtUtc.HasValue ||
                       x.RegistrationStartsAtUtc.Value < x.RegistrationEndsAtUtc.Value)
            .WithMessage("Дата начала регистрации должна быть раньше даты окончания");
    }
}
