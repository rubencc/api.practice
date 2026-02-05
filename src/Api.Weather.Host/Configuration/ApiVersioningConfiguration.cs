using Asp.Versioning;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Weather.Host.Configuration;

public static class ApiVersioningConfiguration
{
    public static IServiceCollection AddApiVersioningConfiguration(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            // Reportar las versiones de API soportadas en los headers de respuesta
            options.ReportApiVersions = true;
            
            // Asumir versión 1.0 si el cliente no especifica una
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            
            // Leer la versión desde la URL (ej: /api/v1/Weather)
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddApiExplorer(options =>
        {
            // Formato del grupo de versión para swagger
            options.GroupNameFormat = "'v'VVV";
            
            // Sustituir la versión en la URL de swagger
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}

