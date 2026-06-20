using Application.CORS.Animal;
using FluentValidation;

namespace Application.Validators.Animal;

public class CreateAnimalCommandValdiator : AbstractValidator<CreateAnimalCommand>
{
    public CreateAnimalCommandValdiator()
    {
        RuleFor(x => x.Model.Name)
            .NotEmpty();

        When(x => string.IsNullOrEmpty(x.Model.Breed), () =>
        {

            RuleFor(x => x.Model.Breed)
            .NotEmpty();
        });

        RuleFor(x => x.Model.Description)
           .NotEmpty();

        RuleFor(x => x.Model.Gender)
           .IsInEnum();

        RuleFor(x => x.Model.Size)
            .IsInEnum();

        RuleFor(x => x.Model.Temperament)
            .IsInEnum();

        RuleFor(x => x.Model.Photos)
            .NotEmpty();
    }
}
