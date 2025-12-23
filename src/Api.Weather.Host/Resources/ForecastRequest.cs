using System;

namespace Api.Weather.Host.Resources;

public class ForecastRequest
{
    public string Address { get; set; }
    public DateOnly Time { get; set; }
}