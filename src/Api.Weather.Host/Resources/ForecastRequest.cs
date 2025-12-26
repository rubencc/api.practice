using System;

namespace Api.Weather.Host.Resources;

public class ForecastRequest
{
    public string Location { get; set; }
    public DateTimeOffset Time { get; set; } = DateTimeOffset.UtcNow;
}