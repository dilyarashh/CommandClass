using FluentValidation;
using PotteryClass.Data.DTOs;

namespace PotteryClass.Infrastructure.Validators;

public class JoinCourseValidator : AbstractValidator<JoinCourseRequest>
{
    public JoinCourseValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty()
            .Length(8)
            .Matches("^[a-z0-9]{8}$")
            .WithMessage("Код должен быть длиной 8 и содержать только a-z и 0-9");
    }
}