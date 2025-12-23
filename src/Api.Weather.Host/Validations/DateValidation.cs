using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Api.Weather.Host.Resources;

namespace Api.Weather.Host.Validations;

public class DateValidation : IValidation<ForecastRequest>
{
    private readonly Regex dateRegex;
    
    public DateValidation()
    {
        this.dateRegex = new Regex(@"^\d{4}/(0[1-9]|1[0-2])/(0[1-9]|[12][0-9]|3[01])$");
    }
    
    public Task<bool> IsValid(ForecastRequest request)
    {
        return Task.FromResult(this.dateRegex.IsMatch(request.Time));
    }
}