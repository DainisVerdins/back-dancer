using FluentValidation;
using WebApi.CORS.Queries;

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
