using System.Reflection;
using MassTransit;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Api.Weather.Consumer.Host.Extensions;

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
            
            //x.AddConsumer<>();

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
                

                configurator.ReceiveEndpoint("api.weather.audit.consumer", e =>
                {
                    e.ConfigureConsumeTopology = false;
                    //e.ConfigureConsumer<>(context);
                    e.SetQuorumQueue();

                    e.UseMessageRetry(r =>
                    {
                        r.Intervals(100, 500, 1000, 2000);
                        r.Ignore(typeof(ArgumentNullException), typeof(ArgumentNullException));
                    });
                });
            });
        });

        return services;
    }
}
