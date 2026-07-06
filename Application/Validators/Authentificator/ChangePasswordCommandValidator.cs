using Application.CORS.Commands.Authentication;
using FluentValidation;

namespace Application.Validators.Authentificator;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {

        RuleFor(x => x.NewPassword)
            .NotEmpty();
        RuleFor(x => x.OldPassword)
            .NotEmpty();
    }
}
