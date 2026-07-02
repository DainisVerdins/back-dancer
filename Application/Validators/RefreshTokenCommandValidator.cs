using Application.CORS.Commands.Authentication;
using FluentValidation;

namespace Application.Validators;

internal class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        When(x => !string.IsNullOrEmpty(x.ActiveRole), () =>
        {
            RuleFor(x => x.ActiveRole)
                .NotEmpty();
        });
    }
}
