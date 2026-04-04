using FluentValidation;
using PotteryClass.Data.DTOs;

namespace PotteryClass.Infrastructure.Validators;

public class CreateCourseValidator : AbstractValidator<CreateCourseRequest>
{
    public CreateCourseValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(2000);

        RuleFor(x => x.RegistrationStartsAtUtc)
            .NotEmpty();

        RuleFor(x => x.RegistrationEndsAtUtc)
            .NotEmpty();

        RuleFor(x => x)
            .Must(x => x.RegistrationStartsAtUtc < x.RegistrationEndsAtUtc)
            .WithMessage("Дата начала регистрации должна быть раньше даты окончания");
    }
}
