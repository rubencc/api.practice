# Configuración de OpenTelemetry en Weather API

## 📋 Descripción

Este proyecto está configurado con **OpenTelemetry** para observabilidad completa, incluyendo:
- ✅ **Trazas distribuidas** (Distributed Tracing)
- ✅ **Métricas** (Metrics)
- ✅ **Integración con Activity.Current** para TraceId en ProblemDetails

## 🚀 Configuración

### 1. Dependencias Instaladas

El proyecto `Wheather.Infrastructure.Opentelemetry` incluye:

```xml
<PackageReference Include="OpenTelemetry.Exporter.Console" Version="1.15.0" />
<PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.15.0" />
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.15.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.15.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.15.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.Runtime" Version="1.15.0" />
```

### 2. Configuración en appsettings.json

```json
{
  "OpenTelemetry": {
    "ServiceName": "WeatherApi",
    "ServiceVersion": "1.0.0",
    "ServiceNamespace": "Weather",
    "Environment": "Development",
    "EnableConsoleExporter": true,
    "EnableOtlpExporter": false,
    "OtlpEndpoint": "http://localhost:4317",
    "EnableAspNetCoreInstrumentation": true,
    "EnableHttpClientInstrumentation": true,
    "EnableRuntimeInstrumentation": true
  }
}
```

### 3. Registro en Program.cs

```csharp
// Configurar OpenTelemetry
builder.Services.AddOpenTelemetryConfiguration(builder.Configuration);
```

## 📊 Funcionalidades

### Trazas Automáticas

OpenTelemetry captura automáticamente:
- ✅ Peticiones HTTP entrantes (ASP.NET Core)
- ✅ Peticiones HTTP salientes (HttpClient)
- ✅ Excepciones y errores
- ✅ Headers, métodos, status codes

### Métricas Automáticas

Se recopilan métricas de:
- ✅ Solicitudes HTTP (rate, duration, errors)
- ✅ HttpClient (latencia, errores)
- ✅ Runtime .NET (GC, threads, memory)

### TraceId en ProblemDetails

Cada respuesta de error incluye el `traceId` de OpenTelemetry:

```json
{
  "type": "https://httpstatuses.com/404",
  "title": "Resource Not Found",
  "status": 404,
  "detail": "Location 'InvalidCity' not found",
  "instance": "/api/v1/weather",
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00f067aa0ba902b7-00"
}
```

Esto permite correlacionar errores con trazas en sistemas de observabilidad.

## 🔧 Uso Avanzado: Trazas Personalizadas

### 1. Inyectar ActivitySource

```csharp
public class ForecastService
{
    private readonly IForecastRepository _repository;
    private readonly ActivitySource _activitySource;

    public ForecastService(
        IForecastRepository repository,
        ActivitySource activitySource)
    {
        _repository = repository;
        _activitySource = activitySource;
    }
}
```

### 2. Crear Spans Personalizados

```csharp
public async Task<bool> AddForecastAsync(
    string location, 
    DateTimeOffset time, 
    ForecastDto dto, 
    CancellationToken cancellationToken = default)
{
    // Crear un span personalizado
    using var activity = _activitySource.StartActivity("AddForecast");
    
    // Agregar tags para contexto
    activity?.SetTag("forecast.location", location);
    activity?.SetTag("forecast.time", time);
    activity?.SetTag("forecast.temperature", dto.Temperature);
    
    try
    {
        var forecast = Forecast.Create(dto.Location, time, dto.Temperature, dto.WeatherDescription);
        var result = await _repository.AddForecastAsync(forecast, cancellationToken);
        
        // Marcar como exitoso
        activity?.SetStatus(ActivityStatusCode.Ok);
        
        return result;
    }
    catch (Exception ex)
    {
        // Registrar error en el span
        activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
        activity?.RecordException(ex);
        throw;
    }
}
```

### 3. Spans Anidados

```csharp
public async Task<ForecastDto> GetForecastAsync(Location location, CancellationToken ct)
{
    using var activity = _activitySource.StartActivity("GetForecast");
    activity?.SetTag("location.address", location.Address);
    
    // Span hijo para geolocalización
    using (var geoActivity = _activitySource.StartActivity("GetCoordinates"))
    {
        geoActivity?.SetTag("location", location.Address);
        // ... llamada al servicio de geolocalización
    }
    
    // Span hijo para datos meteorológicos
    using (var weatherActivity = _activitySource.StartActivity("GetWeatherData"))
    {
        weatherActivity?.SetTag("lat", location.Latitude);
        weatherActivity?.SetTag("lon", location.Longitude);
        // ... llamada al servicio de clima
    }
    
    return forecast;
}
```

## 🎯 Exportadores

### Console Exporter (Desarrollo Local)

Por defecto habilitado en Development. Las trazas se imprimen en la consola:

```
Activity.TraceId:            4bf92f3577b34da6a3ce929d0e0e4736
Activity.SpanId:             00f067aa0ba902b7
Activity.TraceFlags:         Recorded
Activity.ParentSpanId:       0000000000000000
Activity.ActivitySourceName: WeatherApi
Activity.DisplayName:        GET /api/v1/weather
Activity.Kind:               Server
Activity.StartTime:          2025-12-27T12:00:00.0000000Z
Activity.Duration:           00:00:00.1234567
Activity.Tags:
    http.request.method: GET
    http.response.status_code: 200
```

### OTLP Exporter (Integración con Stack de Observabilidad)

El proyecto usa el **Stack Grafana completo** para observabilidad:
- **OpenTelemetry Collector**: Recibe y procesa telemetría
- **Tempo**: Backend de trazas distribuidas
- **Loki**: Agregación de logs
- **Prometheus**: Métricas y alertas
- **Grafana**: Visualización unificada

#### 1. Habilitar OTLP Exporter

Modificar `appsettings.json` o `appsettings.Development.json`:

```json
{
  "OpenTelemetry": {
    "EnableOtlpExporter": true,
    "OtlpEndpoint": "http://localhost:4317"
  }
}
```

#### 2. Iniciar el Stack de Observabilidad

El proyecto ya incluye la configuración en `.docker/docker-compose.yaml` con el perfil `infrastructure-monitoring`:

```bash
# Desde el directorio .docker
cd .docker

# Iniciar solo la infraestructura de monitoreo
docker-compose --profile infrastructure-monitoring up -d

# O iniciar todo (MongoDB + Monitoreo)
docker-compose --profile infrastructure --profile infrastructure-monitoring up -d
```

#### 3. Componentes del Stack

| Servicio | Puerto | URL | Descripción |
|----------|--------|-----|-------------|
| **Grafana** | 3000 | http://localhost:3000 | Dashboard de visualización |
| **Tempo** | 3200 | http://localhost:3200 | Backend de trazas (OTLP: 4317) |
| **Loki** | 3100 | http://localhost:3100 | Agregación de logs |
| **Prometheus** | 9091 | http://localhost:9091 | Métricas |
| **OTEL Collector** | 4317/4318 | - | Receptor OTLP gRPC/HTTP |

#### 4. Configuración del OTEL Collector

El collector ya está configurado en `.docker/monitoring/otel-collector/otel-collector-config.yaml`:

```yaml
receivers:
  otlp:
    protocols:
      grpc:
        endpoint: 0.0.0.0:4317
      http:
        endpoint: 0.0.0.0:4318

processors:
  batch:

exporters:
  otlp/tempo:
    endpoint: tempo:4317
    tls:
      insecure: true
  
  prometheus:
    endpoint: 0.0.0.0:8890

  logging:
    loglevel: debug

service:
  pipelines:
    traces:
      receivers: [otlp]
      processors: [batch]
      exporters: [otlp/tempo, logging]
    
    metrics:
      receivers: [otlp]
      processors: [batch]
      exporters: [prometheus, logging]
```

## 🐳 Arquitectura de Observabilidad

```
┌─────────────────┐
│  Weather API    │
│  (Port 5000)    │
└────────┬────────┘
         │ OTLP (gRPC/HTTP)
         ↓
┌────────────────────────┐
│  OTEL Collector        │
│  (Port 4317/4318)      │
└───┬──────────┬─────────┘
    │          │
    │          ↓
    │    ┌──────────┐
    │    │  Tempo   │ ← Trazas
    │    └──────────┘
    │
    ↓
┌──────────┐
│Prometheus│ ← Métricas
└──────────┘
    ↑
    │
┌──────────────────┐
│    Grafana       │ ← Visualización
│  (Port 3000)     │
└──────────────────┘
```

## 📈 Métricas Disponibles

### ASP.NET Core Metrics
- `http.server.request.duration` - Duración de peticiones
- `http.server.active_requests` - Peticiones activas
- `http.server.request.body.size` - Tamaño del body

### HttpClient Metrics
- `http.client.request.duration` - Duración de peticiones salientes
- `http.client.open_connections` - Conexiones abiertas

### .NET Runtime Metrics
- `process.runtime.dotnet.gc.collections.count` - Recolecciones de GC
- `process.runtime.dotnet.gc.heap.size` - Tamaño del heap
- `process.runtime.dotnet.thread_pool.thread.count` - Threads activos
- `process.runtime.dotnet.assemblies.count` - Ensamblados cargados

## 🔍 Ejemplos de Uso

### 1. Ver Trazas en Consola (Desarrollo Local)

```bash
dotnet run --project src/Api.Weather.Host
```

Salida en consola:
```
Activity.TraceId:          4bf92f3577b34da6a3ce929d0e0e4736
Activity.DisplayName:      POST /api/v1/weather
Activity.Duration:         00:00:01.2345678
Activity.Tags:
    http.request.method: POST
    http.response.status_code: 200
    forecast.location: Madrid
```

### 2. Visualizar en Grafana

#### Iniciar el Stack
```bash
cd .docker
docker-compose --profile infrastructure-monitoring up -d
```

#### Configurar la API para enviar a OTLP
Modificar `appsettings.Development.json`:
```json
{
  "OpenTelemetry": {
    "EnableOtlpExporter": true,
    "OtlpEndpoint": "http://localhost:4317"
  }
}
```

#### Acceder a Grafana
1. Abrir http://localhost:3000
2. Login automático (autenticación anónima habilitada)
3. Ir a **Explore** → Seleccionar **Tempo** como datasource
4. Buscar trazas por:
   - **TraceID**: Pegar el traceId del ProblemDetails
   - **Service Name**: `WeatherApi`
   - **Tags**: `http.method=POST`, `forecast.location=Madrid`

#### Explorar en Tempo
- **Vista de Flamegraph**: Ver duración de cada span
- **Vista de Gantt**: Timeline de operaciones
- **Span Details**: Ver tags, logs, eventos
- **Service Map**: Dependencias entre servicios

### 3. Consultar Métricas en Prometheus

1. Abrir http://localhost:9091
2. Ejecutar queries PromQL:

```promql
# Rate de peticiones HTTP por segundo
rate(http_server_request_duration_seconds_count[5m])

# Latencia P95 de peticiones
histogram_quantile(0.95, rate(http_server_request_duration_seconds_bucket[5m]))

# Peticiones por status code
sum by (http_response_status_code) (rate(http_server_request_duration_seconds_count[5m]))
```

### 4. Dashboards en Grafana

Grafana ya incluye datasources pre-configurados:
- **Tempo**: Para trazas distribuidas
- **Prometheus**: Para métricas
- **Loki**: Para logs (si se configura)

#### Crear Dashboard Personalizado

1. En Grafana → **Dashboards** → **New** → **New Dashboard**
2. Agregar paneles:
   - **Trazas**: Datasource Tempo
   - **Métricas**: Datasource Prometheus
3. Ejemplo de query de métrica:

```promql
# Duración promedio de peticiones a /api/v1/weather
rate(http_server_request_duration_seconds_sum{http_route="/api/v1/weather"}[5m])
/
rate(http_server_request_duration_seconds_count{http_route="/api/v1/weather"}[5m])
```

### 5. Correlacionar Errores con Trazas

Cuando ocurre un error, el cliente recibe:
```json
{
  "traceId": "4bf92f3577b34da6a3ce929d0e0e4736",
  "status": 404,
  "detail": "Location not found"
}
```

**Flujo de debugging**:
1. Copiar el `traceId` del error
2. Ir a Grafana → Explore → Tempo
3. Pegar el traceId en el campo de búsqueda
4. Ver la traza completa:
   - Qué servicios se llamaron
   - Dónde falló exactamente
   - Latencias de cada paso
   - Tags y contexto del error
   - Stack trace si se registró

**Ejemplo de visualización en Grafana/Tempo**:
```
POST /api/v1/weather [500ms] ❌
  ├─ ForecastService.AddForecast [450ms]
  │   ├─ GeolocationService.GetCoordinates [200ms] ✅
  │   ├─ WeatherQueryService.GetForecast [150ms] ✅
  │   └─ ForecastRepository.Save [100ms] ❌ ERROR
  │       └─ MongoDB.InsertOne [90ms] ❌
  │           Error: Connection timeout
```

## 🎓 Mejores Prácticas

### 1. Tags Significativos
```csharp
activity?.SetTag("forecast.location", location);
activity?.SetTag("forecast.temperature.value", temperature.Value);
activity?.SetTag("forecast.temperature.unit", temperature.Unit);
```

### 2. Registrar Excepciones
```csharp
catch (Exception ex)
{
    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
    activity?.RecordException(ex);
    throw;
}
```

### 3. Nombres Descriptivos
```csharp
using var activity = _activitySource.StartActivity(
    "ForecastService.AddForecast",
    ActivityKind.Internal
);
```

### 4. No Abusar de Spans
- Crear spans para operaciones significativas (> 10ms)
- No crear spans para operaciones triviales
- Agrupar operaciones relacionadas

### 5. Tags Cardinales Bajos
```csharp
// ✅ Bien: Valores con cardinalidad baja
activity?.SetTag("forecast.location.country", "Spain");

// ❌ Mal: Valores únicos (alta cardinalidad)
activity?.SetTag("forecast.unique_id", Guid.NewGuid());
```

## 🔒 Seguridad

### No Incluir Datos Sensibles

```csharp
// ❌ MAL: Exponer información sensible
activity?.SetTag("user.api_key", apiKey);
activity?.SetTag("user.password", password);

// ✅ BIEN: Solo metadatos
activity?.SetTag("user.id", userId);
activity?.SetTag("user.role", "admin");
```

## 📚 Referencias

- [OpenTelemetry .NET](https://opentelemetry.io/docs/languages/net/)
- [ASP.NET Core Instrumentation](https://github.com/open-telemetry/opentelemetry-dotnet-contrib/tree/main/src/OpenTelemetry.Instrumentation.AspNetCore)
- [OTLP Protocol](https://opentelemetry.io/docs/specs/otlp/)
- [Grafana Tempo Documentation](https://grafana.com/docs/tempo/latest/)
- [Grafana Loki Documentation](https://grafana.com/docs/loki/latest/)
- [Prometheus Documentation](https://prometheus.io/docs/)
- [OpenTelemetry Collector](https://opentelemetry.io/docs/collector/)
- [Grafana Dashboards](https://grafana.com/docs/grafana/latest/)

---

**Fecha**: 2025-12-27  
**Versión**: 1.1  
**Stack**: Grafana + Tempo + Loki + Prometheus + OTEL Collector

