using System;
using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Api.Weather.Host.Extensions;

internal static class MassTransitExtensions
{
    internal static IServiceCollection AddMassTransit(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMassTransit(x =>
        {
            x.ConfigureHealthCheckOptions(options =>
            {
                options.Name = "masstransit";
                options.MinimalFailureStatus = HealthStatus.Unhealthy;
                options.Tags.Add("readyz");
            });
            
            x.DisableUsageTelemetry();
            
            x.UsingRabbitMq((context, configurator) =>
            {
                var url = new Uri(configuration.GetValue<string>("MassTransit:RabbitMq:Url"));

                configurator.Host(url, c =>
                {
                    c.ConnectionName(Assembly.GetExecutingAssembly().GetName().Name);
                });
                
                configurator.ClearSerialization();
                configurator.AddRawJsonSerializer();
                
                configurator.UseInstrumentation(serviceName: "MassTransit");
            });
        });

        return services;
    }
}
