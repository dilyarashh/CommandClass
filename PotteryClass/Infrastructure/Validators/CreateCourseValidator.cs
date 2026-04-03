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
            .LessThan(x => x.RegistrationEndsAtUtc)
            .WithMessage("Дата начала регистрации должна быть раньше даты окончания");

        RuleFor(x => x.RegistrationEndsAtUtc)
            .GreaterThan(x => x.RegistrationStartsAtUtc)
            .WithMessage("Дата окончания регистрации должна быть позже даты начала");
    }
}
