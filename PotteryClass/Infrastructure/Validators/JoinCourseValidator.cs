using FluentValidation;
using PotteryClass.Data.DTOs;

namespace PotteryClass.Infrastructure.Validators;

public class JoinCourseValidator : AbstractValidator<JoinCourseRequest>
{
    public JoinCourseValidator()
    {
    }
}