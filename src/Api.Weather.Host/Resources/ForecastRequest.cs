namespace Api.Weather.Host.Resources;

public class ForecastRequest
{
    public string PostalCode { get; set; }
    public string Time { get; set; }
}