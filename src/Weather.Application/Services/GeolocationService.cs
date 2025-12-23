using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Weather.Application.Interfaces;

namespace Weather.Application.Services;

public class GeolocationService : IGeolocationService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private const string OpenCageBaseUrl = "https://api.opencagedata.com/geocode/v1/json";

    public GeolocationService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["OpenCage:ApiKey"] ?? throw new InvalidOperationException("OpenCage API Key no configurada");
    }

    public async Task<(string lat, string logn)>  GetCoordinates(string address)
    {
        // Documentación: https://opencagedata.com/api
        
        try
        {
            // Formato: q=codigo_postal+pais&key=API_KEY
            var url = $"{OpenCageBaseUrl}?q={address}+Spain&key={_apiKey}&limit=1&no_annotations=1";
            var response = _httpClient.GetAsync(url).Result;
            
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Error al obtener coordenadas para el código postal {address}. Status: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var geocodingResponse = JsonSerializer.Deserialize<OpenCageResponse>(content);

            if (geocodingResponse == null)
            {
                throw new InvalidOperationException($"OpenCage API error: Respuesta vacía para {address}");
            }

            if (geocodingResponse.Status?.Code != 200)
            {
                throw new InvalidOperationException($"OpenCage API error: {geocodingResponse.Status?.Message}. Código: {geocodingResponse.Status?.Code}");
            }

            if (geocodingResponse.Results == null || geocodingResponse.Results.Count == 0)
            {
                throw new InvalidOperationException($"No se encontraron coordenadas para el código postal {address}");
            }

            var location = geocodingResponse.Results[0].Geometry;
            return (location.Lat.ToString(CultureInfo.InvariantCulture), location.Lng.ToString(CultureInfo.InvariantCulture));
        }
        catch (Exception ex) when (ex is not HttpRequestException && ex is not InvalidOperationException)
        {
            throw new Exception($"Error al obtener coordenadas para {address}: {ex.Message}", ex);
        }
    }

    public void Dispose()
    {
        // El HttpClient se gestiona por el HttpClientFactory, no es necesario hacer Dispose aquí
    }

    #region OpenCage Response Models
    
    private class OpenCageResponse
    {
        [JsonPropertyName("results")]
        public List<OpenCageResult> Results { get; set; } = new();
        
        [JsonPropertyName("status")]
        public OpenCageStatus? Status { get; set; }
        
        [JsonPropertyName("rate")]
        public OpenCageRate? Rate { get; set; }
    }

    private class OpenCageResult
    {
        [JsonPropertyName("geometry")]
        public OpenCageGeometry Geometry { get; set; } = new();
        
        [JsonPropertyName("formatted")]
        public string Formatted { get; set; } = string.Empty;
        
        [JsonPropertyName("components")]
        public OpenCageComponents? Components { get; set; }
    }

    private class OpenCageGeometry
    {
        [JsonPropertyName("lat")]
        public double Lat { get; set; }
        
        [JsonPropertyName("lng")]
        public double Lng { get; set; }
    }

    private class OpenCageStatus
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }
        
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }

    private class OpenCageRate
    {
        [JsonPropertyName("limit")]
        public int Limit { get; set; }
        
        [JsonPropertyName("remaining")]
        public int Remaining { get; set; }
        
        [JsonPropertyName("reset")]
        public long Reset { get; set; }
    }

    private class OpenCageComponents
    {
        [JsonPropertyName("postcode")]
        public string? PostCode { get; set; }
        
        [JsonPropertyName("country")]
        public string? Country { get; set; }
    }
    
    #endregion
}

