using System;
using Api.Weather.Host.Resources;
using FluentValidation;

namespace Api.Weather.Host.Validations;

public class ForecastRequestValidator : AbstractValidator<ForecastRequest>
{
    public ForecastRequestValidator()
    {
        
        RuleFor(x => x)
            .NotNull()
            .WithMessage("Forecast request cannot be null");
        
        RuleFor(x => x.Location)
            .Cascade(CascadeMode.Continue)
            .NotEmpty()
            .WithMessage("Location is required")
            .MaximumLength(200)
            .WithMessage("Location must not exceed 200 characters");

        // Validar Time
        RuleFor(x => x.Time)
            .Cascade(CascadeMode.Continue)
            .NotEmpty()
            .WithMessage("Time is required")
            .Must(TimeMustNotBeDefault)
            .WithMessage("Time must not be default value");
    }
    
    private bool TimeMustNotBeDefault(DateTimeOffset time)
    {
        return time != default;
    }
}