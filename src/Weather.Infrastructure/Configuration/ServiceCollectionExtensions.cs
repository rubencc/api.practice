using Microsoft.Extensions.DependencyInjection;
using Weather.Domain.Repositories;
using Weather.Infrastructure.Persistence.Repositories;

namespace Weather.Infrastructure.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureDependencies(this IServiceCollection services)
    {
        services.AddTransient<IForecastRepository, ForecastRepository>();
        return services;
    }
}