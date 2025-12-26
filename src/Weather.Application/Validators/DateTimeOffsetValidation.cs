using Weather.Application.Commands;
using Weather.Application.Interfaces;

namespace Weather.Application.Validators;

public class DateTimeOffsetValidation : IValidation<ForecastCommand>
{
    public Task<bool> IsValid(ForecastCommand command)
    {
        return Task.FromResult(default != command.Time);
    }
}