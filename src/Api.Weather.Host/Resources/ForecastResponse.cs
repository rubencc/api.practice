using System;

namespace Api.Weather.Host.Resources;

public class ForecastResponse
{
    public string Location { get; set; }
    public string Time { get; set; }
    public string Temperature { get; set; }
    public string Weather { get; set; }
    public string CreatedAt => DateTime.UtcNow.ToString();
}