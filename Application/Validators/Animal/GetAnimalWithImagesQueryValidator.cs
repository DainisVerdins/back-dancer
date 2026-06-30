using Application.CORS.Queries;
using FluentValidation;

namespace Application.Validators.Animal;

public class GetAnimalWithImagesQueryValidator : AbstractValidator<GetAnimalWithImagesQuery>
{
    public GetAnimalWithImagesQueryValidator()
    {

        RuleFor(x => x.AnimalId)
                .NotEmpty()
                .GreaterThan(0);
    }
}
