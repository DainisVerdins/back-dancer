using Application.CORS.Queries;
using FluentValidation;

namespace WebApi.Validators;

public class GetWeatherForecastQueryValidator : AbstractValidator<GetWeatherForecastQuery>
{
    public GetWeatherForecastQueryValidator()
    {
        RuleFor(x => x.MaxNumberOfForecastToReturn)
            .NotNull()
            .GreaterThan(0);
    }
}
