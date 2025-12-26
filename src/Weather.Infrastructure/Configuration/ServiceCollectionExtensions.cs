using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Weather.Domain.Repositories;
using Weather.Infrastructure.Persistence.Repositories;

namespace Weather.Infrastructure.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureDependencies(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configurar MongoDB
        
        services.AddOptions<MongoDbSettings>()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.Configure<MongoDbSettings>(configuration.GetSection(MongoDbSettings.SECTION_KEY));

        services.AddSingleton<IMongoClient>(sp =>
            {
                var options = sp.GetRequiredService<IOptions<MongoDbSettings>>();
                var clientSettings = MongoClientSettings.FromConnectionString(options.Value.ConnectionString);
                clientSettings.ApplicationName = AppDomain.CurrentDomain.FriendlyName;
            
                return new MongoClient(clientSettings);
            })
            .AddTransient(sp =>
            {
                var options = sp.GetRequiredService<IOptions<MongoDbSettings>>();
                var mongoClient = sp.GetRequiredService<IMongoClient>();
                return mongoClient.GetDatabase(options.Value.DatabaseName);
            });
        
        // Registrar repositorios
        services.AddTransient<IForecastRepository, ForecastRepository>();
        
        return services;
    }
}

