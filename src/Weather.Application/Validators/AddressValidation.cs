using Weather.Application.Commands;
using Weather.Application.Interfaces;

namespace Weather.Application.Validators;

public class AddressValidation : IValidation<ForecastCommand>
{
    public Task<bool> IsValid(ForecastCommand command)
    {
        return Task.FromResult(!string.IsNullOrWhiteSpace(command.Address));
    }
}