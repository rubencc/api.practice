using Microsoft.Extensions.DependencyInjection;
using Weather.Application.Commands;
using Weather.Application.Interfaces;
using Weather.Application.Services;
using Weather.Application.Validators;

namespace Weather.Application.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationDependencies(this IServiceCollection services)
    {
        // Registrar validaciones
        services.AddTransient<IValidation<ForecastCommand>, AddressValidation>();
        services.AddTransient<IValidation<ForecastCommand>, DateValidation>();

        services.AddTransient<IGeolocationService, GeolocationService>();
        services.AddTransient<IWeatherQueryService, WeatherQueryService>();

        services.AddHttpClients();

        services.AddTransient<ForecastService>();
        
        return services;
    }
}