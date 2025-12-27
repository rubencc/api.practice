using Weather.Domain.ValueObjects;

namespace Weather.Application.DTOs;

public class ForecastDto
{
    public Location Location { get; set; }
    public DateTime Time { get; set; }
    public Temperature Temperature { get; set; }
    public double WindSpeed { get; set; }
    public int WindDirection { get; set; }
    public int WeatherCode { get; set; }
    public string WeatherDescription { get; set; } = string.Empty;
}

