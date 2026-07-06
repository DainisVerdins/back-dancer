using Application.CORS.Commands;
using FluentValidation;

namespace Application.Validators.Authentificator;

public class SelectUserRoleCommandValidator : AbstractValidator<SelectUserRoleCommand>
{
    public SelectUserRoleCommandValidator()
    {
        RuleFor(x => x.RoleCode)
            .NotEmpty();
    }
}
