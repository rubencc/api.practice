using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics;
using System.Reflection;
using Microsoft.Extensions.Options;

namespace Weather.Infrastructure.Opentelemetry.Configuration;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configura OpenTelemetry con trazas, métricas y exportadores
    /// </summary>
    public static IServiceCollection AddOpenTelemetryConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        
        services.AddOptions<OpenTelemetryOptions>()
            .Bind(configuration.GetSection("OpenTelemetry"))
            .Configure(options =>
            {
                var assembly = Assembly.GetEntryAssembly()!;
                var entryAssemblyName = assembly.GetName();
                options.ServiceName = entryAssemblyName.Name!;
                options.ServiceVersion = entryAssemblyName.Version!.ToString();
                options.ServiceNamespace = assembly.EntryPoint?.DeclaringType?.Namespace!;
            })
            .ValidateDataAnnotations();
        

        var options = services.BuildServiceProvider().GetRequiredService<IOptions<OpenTelemetryOptions>>().Value;
        
        // Crear recurso compartido con información del servicio
        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(
                serviceName: options.ServiceName,
                serviceVersion: options.ServiceVersion,
                serviceNamespace: options.ServiceNamespace)
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment"] = options.Environment,
                ["host.name"] = Environment.MachineName
            });

        // Configurar OpenTelemetry
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                serviceName: options.ServiceName,
                serviceVersion: options.ServiceVersion))
            .WithTracing(tracerProviderBuilder =>
            {
                tracerProviderBuilder
                    .SetResourceBuilder(resourceBuilder)
                    .AddSource(options.ServiceName) // Para Activity.StartActivity
                    .SetSampler(new AlwaysOnSampler()); // Samplear todas las trazas

                // Instrumentación de ASP.NET Core
                if (options.EnableAspNetCoreInstrumentation)
                {
                    tracerProviderBuilder.AddAspNetCoreInstrumentation(aspNetCoreOptions =>
                    {
                        // Enriquecer con información adicional
                        aspNetCoreOptions.RecordException = true;
                        aspNetCoreOptions.EnrichWithHttpRequest = (activity, httpRequest) =>
                        {
                            activity.SetTag("http.request.user_agent", httpRequest.Headers.UserAgent.ToString());
                            activity.SetTag("http.request.content_length", httpRequest.ContentLength);
                        };
                        aspNetCoreOptions.EnrichWithHttpResponse = (activity, httpResponse) =>
                        {
                            activity.SetTag("http.response.content_length", httpResponse.ContentLength);
                        };
                    });
                }

                // Instrumentación de HttpClient
                if (options.EnableHttpClientInstrumentation)
                {
                    tracerProviderBuilder.AddHttpClientInstrumentation(httpClientOptions =>
                    {
                        httpClientOptions.RecordException = true;
                        httpClientOptions.EnrichWithHttpRequestMessage = (activity, httpRequestMessage) =>
                        {
                            activity.SetTag("http.request.method", httpRequestMessage.Method.ToString());
                            activity.SetTag("http.request.uri", httpRequestMessage.RequestUri?.ToString());
                        };
                    });
                }

                // Exportadores
                if (options.EnableConsoleExporter)
                {
                    tracerProviderBuilder.AddConsoleExporter();
                }

                if (options.EnableOtlpExporter)
                {
                    tracerProviderBuilder.AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri(options.OtlpEndpoint);
                        otlpOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                    });
                }
            })
            .WithMetrics(meterProviderBuilder =>
            {
                meterProviderBuilder
                    .SetResourceBuilder(resourceBuilder)
                    .AddMeter(options.ServiceName); // Para métricas personalizadas

                // Instrumentación de ASP.NET Core (métricas)
                if (options.EnableAspNetCoreInstrumentation)
                {
                    meterProviderBuilder.AddAspNetCoreInstrumentation();
                }

                // Instrumentación de HttpClient (métricas)
                if (options.EnableHttpClientInstrumentation)
                {
                    meterProviderBuilder.AddHttpClientInstrumentation();
                }

                // Métricas de runtime .NET
                if (options.EnableRuntimeInstrumentation)
                {
                    meterProviderBuilder.AddRuntimeInstrumentation();
                }

                if (options.EnableOtlpExporter)
                {
                    meterProviderBuilder.AddOtlpExporter(otlpOptions =>
                    {
                        otlpOptions.Endpoint = new Uri(options.OtlpEndpoint);
                        otlpOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                    });
                }
            });

        // Registrar ActivitySource para crear spans personalizados
        services.AddSingleton(new ActivitySource(options.ServiceName, options.ServiceVersion));

        return services;
    }
}

