using Microsoft.Extensions.DependencyInjection;
using Weather.Application.Interfaces;
using Weather.Application.Services;

namespace Weather.Application.Configuration;

public static class HttpClientExtensions
{
    public static IServiceCollection AddHttpClients(this IServiceCollection services)
    {
        // Configurar HttpClient para GeolocationService con OpenCage Geocoding API
        services.AddHttpClient<IGeolocationService, GeolocationService>(client =>
        {
            client.BaseAddress = new Uri("https://api.opencagedata.com/geocode/v1/");
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .AddStandardResilienceHandler(options =>
        {
            // Retry: 3 intentos con backoff exponencial
            options.Retry.MaxRetryAttempts = 3;
            options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;
            options.Retry.UseJitter = true;
            
            // Circuit Breaker: Abre el circuito después de 50% de fallos
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
            options.CircuitBreaker.FailureRatio = 0.5;
            options.CircuitBreaker.MinimumThroughput = 5;
            options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);
            
            // Timeouts
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
        });

        // Configurar HttpClient para WeatherQueryService con Open-Meteo API
        services.AddHttpClient<IWeatherQueryService, WeatherQueryService>(client =>
        {
            client.BaseAddress = new Uri("https://api.open-meteo.com/v1/");
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.DefaultRequestHeaders.Add("User-Agent", "WeatherAPI/1.0");
        })
        .AddStandardResilienceHandler(options =>
        {
            // Retry: 3 intentos con backoff exponencial
            options.Retry.MaxRetryAttempts = 3;
            options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;
            options.Retry.UseJitter = true;
            
            // Circuit Breaker
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromSeconds(30);
            options.CircuitBreaker.FailureRatio = 0.5;
            options.CircuitBreaker.MinimumThroughput = 5;
            options.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(30);
            
            // Timeouts
            options.AttemptTimeout.Timeout = TimeSpan.FromSeconds(10);
            options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);
        });

        return services;
    }
}

