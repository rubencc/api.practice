using Microsoft.Extensions.DependencyInjection;
using Weather.Application.Interfaces;
using Weather.Application.Services;

namespace Weather.Application.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
    {
        services.AddTransient<IGeolocationService, GeolocationService>();
        services.AddTransient<IWeatherQueryService, WeatherQueryService>();

        services.AddHttpClients();

        services.AddTransient<ForecastService>();
        
        return services;
    }
}