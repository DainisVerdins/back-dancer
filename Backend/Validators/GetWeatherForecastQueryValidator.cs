using Backend.CORS.Queries;
using FluentValidation;

namespace Backend.Validators;

public class GetWeatherForecastQueryValidator : AbstractValidator<GetWeatherForecastQuery>
{
    public GetWeatherForecastQueryValidator()
    {
        RuleFor(x => x.MaxNumberOfForecastToReturn)
            .NotNull()
            .GreaterThan(0); 
    }
}
