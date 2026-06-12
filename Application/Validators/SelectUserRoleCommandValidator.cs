using Application.CORS.Commands;
using FluentValidation;

namespace Application.Validators;

public class SelectUserRoleCommandValidator : AbstractValidator<SelectUserRoleCommand>
{
    public SelectUserRoleCommandValidator()
    {
        RuleFor(x => x.RoleCode)
            .NotEmpty();
    }
}
