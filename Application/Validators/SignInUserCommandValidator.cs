using Application.CORS.Commands.Authentication;
using FluentValidation;

namespace Application.Validators;

public class SignInUserCommandValidator : AbstractValidator<SignInUserCommand>
{
    public SignInUserCommandValidator()
    {
        RuleFor(x => x.Model.Email)
            .EmailAddress()
            .NotEmpty();

        RuleFor(x => x.Model.Password)
            .NotEmpty();
    }
}
