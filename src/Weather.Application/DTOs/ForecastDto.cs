namespace Weather.Application.DTOs;

public class ForecastDto
{
    public string Latitude { get; set; } = string.Empty;
    public string Longitude { get; set; } = string.Empty;
    public DateTime Time { get; set; }
    public double Temperature { get; set; }
    public double WindSpeed { get; set; }
    public int WindDirection { get; set; }
    public int WeatherCode { get; set; }
    public string WeatherDescription { get; set; } = string.Empty;
}

