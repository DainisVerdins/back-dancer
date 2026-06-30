using Application.CORS.Queries;
using FluentValidation;

namespace Application.Validators.Animal;

public class GetAllAnimalsQueryValidator : AbstractValidator<GetAllAnimalsQuery>
{
    public GetAllAnimalsQueryValidator()
    {
        RuleFor(x => x.Paging.PageNumber)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Paging.PageSize)
            .GreaterThan(0);


        When(x => !string.IsNullOrEmpty(x.Filter.NameSearchTerm), () =>
        {
            RuleFor(x => x.Filter.NameSearchTerm)
                .NotEmpty();
        });
        When(x => !string.IsNullOrEmpty(x.Filter.BreedSearchTerm), () =>
        {
            RuleFor(x => x.Filter.BreedSearchTerm)
                .NotEmpty();
        });

        When(x => x.Filter.AdmissionDate != null, () =>
        {
            RuleFor(x => x.Filter.AdmissionDate)
                .NotEmpty();
        });

        When(x => x.Filter.Status != null, () =>
        {
            RuleFor(x => x.Filter.Status)
                .IsInEnum();
        });

        When(x => x.Filter.Size != null, () =>
        {
            RuleFor(x => x.Filter.Size)
                .IsInEnum();
        });

        When(x => x.Filter.Gender != null, () =>
        {
            RuleFor(x => x.Filter.Gender)
                .IsInEnum();
        });

        When(x => x.Filter.Temperament != null, () =>
        {
            RuleFor(x => x.Filter.Temperament)
                .IsInEnum();
        });
    }
}
