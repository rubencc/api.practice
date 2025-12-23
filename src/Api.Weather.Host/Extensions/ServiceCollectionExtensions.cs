using Api.Weather.Host.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Weather.Host.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDependencies(this IServiceCollection services)
    {
        services.AddSingleton<ForecastService>();
        return services;
    }
}