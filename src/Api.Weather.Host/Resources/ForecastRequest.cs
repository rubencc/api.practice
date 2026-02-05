using System;

namespace Api.Weather.Host.Resources;

public class ForecastRequest
{
    public string Location { get; set; }
    public DateTime Time { get; set; } = DateTime.UtcNow;
}