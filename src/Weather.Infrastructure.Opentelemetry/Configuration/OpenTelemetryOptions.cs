namespace Weather.Infrastructure.Opentelemetry.Configuration;

/// <summary>
/// Opciones de configuración para OpenTelemetry
/// </summary>
public class OpenTelemetryOptions
{
    /// <summary>
    /// Nombre del servicio
    /// </summary>
    public string ServiceName { get; set; }
    
    /// <summary>
    /// Versión del servicio
    /// </summary>
    public string ServiceVersion { get; set; } 
    
    /// <summary>
    /// Namespace del servicio
    /// </summary>
    public string ServiceNamespace { get; set; } 
    
    /// <summary>
    /// Ambiente de ejecución (Development, Staging, Production)
    /// </summary>
    public string Environment { get; set; }
    
    /// <summary>
    /// Habilitar exportación a consola (útil para desarrollo)
    /// </summary>
    public bool EnableConsoleExporter { get; set; } = true;
    
    /// <summary>
    /// Habilitar exportación OTLP (para Jaeger, Grafana Tempo, etc.)
    /// </summary>
    public bool EnableOtlpExporter { get; set; } = false;
    
    /// <summary>
    /// Endpoint del exportador OTLP (ej: http://localhost:4317)
    /// </summary>
    public string OtlpEndpoint { get; set; } = "http://localhost:4317";
    
    /// <summary>
    /// Habilitar instrumentación de ASP.NET Core
    /// </summary>
    public bool EnableAspNetCoreInstrumentation { get; set; } = true;
    
    /// <summary>
    /// Habilitar instrumentación de HttpClient
    /// </summary>
    public bool EnableHttpClientInstrumentation { get; set; } = true;
    
    /// <summary>
    /// Habilitar métricas de runtime (.NET)
    /// </summary>
    public bool EnableRuntimeInstrumentation { get; set; } = true;
}

