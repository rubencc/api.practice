namespace Api.Practice.Extensions;

using Api.Practice.Resources;
using Api.Practice.Services;
using Api.Practice.Validations;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDependencies(this IServiceCollection services)
    {

        services.AddSingleton<ForecastService>();
        return services;
    }
}