using System.Text.RegularExpressions;
using Weather.Application.Commands;
using Weather.Application.Interfaces;

namespace Weather.Application.Validators;

public class DateValidation : IValidation<ForecastCommand>
{
    private readonly Regex dateRegex;
    
    public DateValidation()
    {
        this.dateRegex = new Regex(@"^\d{4}-(0[1-9]|1[0-2])-(0[1-9]|[12][0-9]|3[01])$");
    }
    
    public Task<bool> IsValid(ForecastCommand command)
    {
        return Task.FromResult(this.dateRegex.IsMatch(command.Time.ToString("O")));
    }
}