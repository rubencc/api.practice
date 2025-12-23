using System.Text.Json;
using System.Text.Json.Serialization;
using Weather.Application.DTOs;
using Weather.Application.Interfaces;

namespace Weather.Application.Services;

public class WeatherQueryService : IWeatherQueryService
{
    private readonly HttpClient _httpClient;
    private const string OpenMeteoBaseUrl = "https://api.open-meteo.com/v1/forecast";

    public WeatherQueryService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ForecastDto> GetForecastAsync(
        (string latitude, string longitude) info, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Open-Meteo API: https://open-meteo.com/en/docs
            // Parámetros: current_weather=true para clima actual
            var url = $"{OpenMeteoBaseUrl}?latitude={info.latitude}&longitude={info.longitude}&current_weather=true&timezone=auto";
            
            var response = await _httpClient.GetAsync(url, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Error al obtener el clima para coordenadas ({info.latitude}, {info.longitude}). Status: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var weatherResponse = JsonSerializer.Deserialize<OpenMeteoResponse>(content);

            if (weatherResponse?.CurrentWeather == null)
            {
                throw new InvalidOperationException(
                    $"No se encontró información del clima para las coordenadas ({info.latitude}, {info.longitude})");
            }

            return new ForecastDto
            {
                Latitude = info.latitude,
                Longitude = info.longitude,
                Time = DateTime.Parse(weatherResponse.CurrentWeather.Time),
                Temperature = weatherResponse.CurrentWeather.Temperature,
                WindSpeed = weatherResponse.CurrentWeather.WindSpeed,
                WindDirection = weatherResponse.CurrentWeather.WindDirection,
                WeatherCode = weatherResponse.CurrentWeather.WeatherCode,
                WeatherDescription = GetWeatherDescription(weatherResponse.CurrentWeather.WeatherCode)
            };
        }
        catch (Exception ex) when (ex is not HttpRequestException && ex is not InvalidOperationException)
        {
            throw new Exception($"Error al obtener el clima: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<ForecastDto>> GetHistoricalForecastsAsync(
        string location, 
        CancellationToken cancellationToken = default)
    {
        // Para obtener pronósticos históricos, necesitarías las coordenadas y fechas
        // Esta implementación es un placeholder
        await Task.CompletedTask;
        throw new NotImplementedException(
            "Para obtener pronósticos históricos, usa la API de Open-Meteo con parámetros start_date y end_date");
    }

    public void Dispose()
    {
        // El HttpClient se gestiona por HttpClientFactory, no requiere dispose manual
    }

    /// <summary>
    /// Convierte el código WMO Weather interpretation a descripción en español
    /// Referencia: https://open-meteo.com/en/docs
    /// </summary>
    private static string GetWeatherDescription(int weatherCode)
    {
        return weatherCode switch
        {
            0 => "Despejado",
            1 => "Principalmente despejado",
            2 => "Parcialmente nublado",
            3 => "Nublado",
            45 => "Niebla",
            48 => "Niebla con escarcha",
            51 => "Llovizna ligera",
            53 => "Llovizna moderada",
            55 => "Llovizna densa",
            56 => "Llovizna helada ligera",
            57 => "Llovizna helada densa",
            61 => "Lluvia ligera",
            63 => "Lluvia moderada",
            65 => "Lluvia intensa",
            66 => "Lluvia helada ligera",
            67 => "Lluvia helada intensa",
            71 => "Nieve ligera",
            73 => "Nieve moderada",
            75 => "Nieve intensa",
            77 => "Granos de nieve",
            80 => "Chubascos ligeros",
            81 => "Chubascos moderados",
            82 => "Chubascos violentos",
            85 => "Chubascos de nieve ligeros",
            86 => "Chubascos de nieve intensos",
            95 => "Tormenta",
            96 => "Tormenta con granizo ligero",
            99 => "Tormenta con granizo intenso",
            _ => "Desconocido"
        };
    }

    #region Open-Meteo Response Models

    private class OpenMeteoResponse
    {
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }

        [JsonPropertyName("current_weather")]
        public CurrentWeather? CurrentWeather { get; set; }
    }

    private class CurrentWeather
    {
        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }

        [JsonPropertyName("windspeed")]
        public double WindSpeed { get; set; }

        [JsonPropertyName("winddirection")]
        public int WindDirection { get; set; }

        [JsonPropertyName("weathercode")]
        public int WeatherCode { get; set; }

        [JsonPropertyName("time")]
        public string Time { get; set; } = string.Empty;
    }

    #endregion
}

