using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Weather.Infrastructure.Cache.Services;

namespace Weather.Infrastructure.Cache.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRedisCaching(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        
        var useRedis = configuration.GetValue<bool>("Redis:Enabled");
        
        if (useRedis)
        {
            // Configurar RedisSettings con Options Pattern
            services.Configure<RedisSettings>(
                configuration.GetSection(RedisSettings.SectionName));

            var redisSettings = configuration
                .GetSection(RedisSettings.SectionName)
                .Get<RedisSettings>();

            if (redisSettings?.Enabled != true) return services;

            var configurationOptions = ConfigurationOptions.Parse(redisSettings.ConnectionString);
            configurationOptions.ConnectTimeout = redisSettings.ConnectTimeout;
            configurationOptions.SyncTimeout = redisSettings.SyncTimeout;
            configurationOptions.AbortOnConnectFail = redisSettings.AbortOnConnectFail;
            // configurationOptions.ConnectRetry = true;
            // configurationOptions.AllowAdmin = redisSettings.AllowAdmin;

            services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(configurationOptions));

            services.AddSingleton<ICacheService, RedisCacheService>();
        }
        else
        {
            services.AddSingleton<ICacheService, MemoryCacheService>();
        }

        return services;
    }
}
