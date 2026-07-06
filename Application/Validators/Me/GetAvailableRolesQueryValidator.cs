using Application.CORS.Queries;
using FluentValidation;

namespace Application.Validators.Me;

internal class GetAvailableRolesQueryValidator : AbstractValidator<GetAvailableRolesQuery>
{
    public GetAvailableRolesQueryValidator()
    {
        RuleFor(x => x.UserId)
                .NotEmpty();
    }
}
