using Application.CORS.Animal;
using FluentValidation;

namespace Application.Validators.Animal;

public class DeleteAnimalCommandValidator : AbstractValidator<DeleteAnimalCommand>
{
    public DeleteAnimalCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .GreaterThan(0);
    }
}
