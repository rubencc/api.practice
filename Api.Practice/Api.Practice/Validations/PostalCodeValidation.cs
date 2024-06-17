namespace Api.Practice.Validations;

using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Api.Practice.Resources;

public class PostalCodeValidation : IValidation<ForecastRequest>
{
    private readonly Regex postalCodeRegex;
    private readonly Regex dateRegex;


    public PostalCodeValidation()
    {
        this.postalCodeRegex = new Regex(@"^(0[1-9]|[1-4][0-9]|5[0-2])\d{3}$");
        this.dateRegex = new Regex(@"^\d{4}/(0[1-9]|1[0-2])/(0[1-9]|[12][0-9]|3[01])$");
    }
    
    public Task<bool> IsValid(ForecastRequest request)
    {
        return Task.FromResult(this.postalCodeRegex.IsMatch(request.PostalCode) && this.dateRegex.IsMatch(request.Time));
    }
}