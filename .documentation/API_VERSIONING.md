# API Versioning - Documentación

## 📋 Implementación

Se ha implementado el **versionado de API en la URL** utilizando `Asp.Versioning` (antes conocido como Microsoft.AspNetCore.Mvc.Versioning), siguiendo las mejores prácticas de .NET 8.

**Características clave**:
- ✅ Versionado en la URL (ej: `/api/v1/Weather`)
- ✅ Configuración modular separada del `Program.cs`
- ✅ Swagger con soporte para múltiples versiones
- ✅ Versión por defecto: 1.0

---

## 🏗️ Arquitectura

### Componentes Implementados

```
Api.Weather.Host/
├── Configuration/
│   ├── ApiVersioningConfiguration.cs    # Configuración de versionado
│   └── SwaggerConfiguration.cs          # Configuración de Swagger multi-versión
├── Controllers/
│   └── WeatherController.cs             # Controller con versión 1.0
└── Program.cs                           # Entry point (sin configuración extra)
```

---

## 🔧 Configuración

### 1. Paquetes NuGet

```xml
<PackageReference Include="Asp.Versioning.Http" Version="8.1.0" />
<PackageReference Include="Asp.Versioning.Mvc.ApiExplorer" Version="8.1.0" />
```

### 2. ApiVersioningConfiguration.cs

```csharp
public static class ApiVersioningConfiguration
{
    public static IServiceCollection AddApiVersioningConfiguration(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            // Reportar versiones soportadas en headers
            options.ReportApiVersions = true;
            
            // Versión por defecto si no se especifica
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
            
            // Leer la versión desde la URL
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddApiExplorer(options =>
        {
            // Formato del grupo de versión
            options.GroupNameFormat = "'v'VVV";
            
            // Sustituir versión en URL de swagger
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}
```

### 3. SwaggerConfiguration.cs

```csharp
public static class SwaggerConfiguration
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
        services.AddSwaggerGen();

        return services;
    }
}

public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _provider;

    public void Configure(SwaggerGenOptions options)
    {
        // Crear un documento de Swagger por cada versión
        foreach (var description in _provider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(
                description.GroupName,
                CreateInfoForApiVersion(description));
        }
    }
}
```

### 4. Program.cs (Limpio y Simple)

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddEndpointsApiExplorer()
    .AddControllers();

// Configuraciones modulares
builder.Services.AddApiVersioningConfiguration();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddSwaggerConfiguration();

builder.Services
    .AddApplicationDependencies()
    .AddInfrastructureDependencies(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.UseHttpsRedirection();

// Swagger UI con múltiples versiones
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
    
    foreach (var description in provider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint(
            $"/swagger/{description.GroupName}/swagger.json",
            description.GroupName.ToUpperInvariant());
    }
});

app.UseRouting();
app.MapControllers();

app.Run();
```

---

## 📝 Uso en Controllers

### Controller con versión específica

```csharp
using Asp.Versioning;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class WeatherController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> GetForecast([FromBody] ForecastRequest request)
    {
        // Implementación
    }
}
```

**URL resultante**: `/api/v1/Weather`

### Controller con múltiples versiones

```csharp
[ApiVersion("1.0")]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class WeatherController : ControllerBase
{
    [HttpPost]
    [MapToApiVersion("1.0")]
    public async Task<IActionResult> GetForecastV1([FromBody] ForecastRequest request)
    {
        // Implementación v1
    }

    [HttpPost]
    [MapToApiVersion("2.0")]
    public async Task<IActionResult> GetForecastV2([FromBody] ForecastRequestV2 request)
    {
        // Implementación v2 con nuevas características
    }
}
```

### Deprecar una versión

```csharp
[ApiVersion("1.0", Deprecated = true)]
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiController]
public class WeatherController : ControllerBase
{
    // ...
}
```

---

## 🌐 Endpoints Disponibles

### Versión 1.0 (Actual)

```
POST /api/v1/Weather
GET  /api/v1/Weather/{id}  (futuro)
```

### Headers de Respuesta

Todas las respuestas incluyen headers con las versiones soportadas:

```http
api-supported-versions: 1.0
api-deprecated-versions: (vacío)
```

---

## 📊 Swagger UI

### Múltiples Versiones

Swagger UI muestra un dropdown para seleccionar la versión:

```
┌─────────────────────────────┐
│ Select a definition         │
│ ▼ V1                       │
│   V2 (si existe)           │
└─────────────────────────────┘
```

### Acceso a Swagger

- **UI**: https://localhost:5001/swagger
- **JSON v1**: https://localhost:5001/swagger/v1/swagger.json
- **JSON v2**: https://localhost:5001/swagger/v2/swagger.json (cuando se implemente)

---

## 🧪 Ejemplos de Uso

### Request a versión 1.0

```bash
curl -X POST https://localhost:5001/api/v1/Weather \
  -H "Content-Type: application/json" \
  -d '{
    "location": "Madrid, España",
    "time": "2025-12-26T10:00:00Z"
  }' \
  -k -i
```

**Response Headers**:
```http
HTTP/2 200
content-type: application/json; charset=utf-8
api-supported-versions: 1.0
```

### Request sin especificar versión (usa default)

```bash
curl -X POST https://localhost:5001/api/Weather \
  -H "Content-Type: application/json" \
  -d '{"location":"Madrid, España","time":"2025-12-26T10:00:00Z"}' \
  -k
```

**Nota**: Como `AssumeDefaultVersionWhenUnspecified = true`, esto funciona y usa v1.0.

### Request a versión no soportada

```bash
curl -X POST https://localhost:5001/api/v99/Weather \
  -H "Content-Type: application/json" \
  -d '{"location":"Madrid","time":"2025-12-26T10:00:00Z"}' \
  -k -i
```

**Response**:
```json
{
  "type": "https://httpstatuses.com/400",
  "title": "Invalid Argument",
  "status": 400,
  "detail": "The HTTP resource that matches the request URI 'https://localhost:5001/api/v99/Weather' does not support the API version '99'.",
  "instance": "/api/v99/Weather",
  "traceId": "00-..."
}
```

---

## 🎯 Estrategias de Versionado

### 1. URL Segment (Implementado)

```
/api/v1/Weather
/api/v2/Weather
```

**✅ Ventajas**:
- Claro y explícito
- Fácil de cachear
- RESTful
- Visible en Swagger

**❌ Desventajas**:
- Más verboso

### 2. Query String (No implementado)

```
/api/Weather?api-version=1.0
```

### 3. Header (No implementado)

```http
GET /api/Weather
api-version: 1.0
```

### 4. Media Type (No implementado)

```http
GET /api/Weather
Accept: application/json;v=1.0
```

---

## 📐 Mejores Prácticas

### 1. Versionado Semántico

- **Major**: Cambios que rompen compatibilidad → Nueva versión (v1 → v2)
- **Minor**: Nuevas características compatible → Mismo endpoint
- **Patch**: Bug fixes → Mismo endpoint

### 2. Cuándo crear una nueva versión

✅ **Sí**:
- Cambios en el contrato de request/response
- Eliminación de campos
- Cambios en el comportamiento esperado
- Cambios en códigos de estado HTTP

❌ **No**:
- Agregar campos opcionales al response
- Nuevos endpoints
- Bug fixes
- Mejoras de rendimiento

### 3. Deprecación gradual

```csharp
[ApiVersion("1.0", Deprecated = true)]
[ApiVersion("2.0")]
```

**Flujo de deprecación**:
1. Anunciar deprecación (3-6 meses antes)
2. Marcar como deprecated en código
3. Documentar en Swagger
4. Remover después del período de gracia

### 4. Documentación de cambios

Cada versión debe documentar:
- Qué cambió
- Por qué cambió
- Cómo migrar
- Fecha de deprecación (si aplica)

---

## 🔮 Roadmap de Versiones

### v1.0 (Actual) ✅
- Endpoint POST /api/v1/Weather
- Búsqueda por ubicación y fecha
- Almacenamiento en MongoDB
- ProblemDetails con OpenTelemetry

### v1.1 (Planificado)
- GET /api/v1/Weather/{id} - Obtener pronóstico por ID
- GET /api/v1/Weather/history - Histórico de consultas
- Paginación de resultados

### v2.0 (Futuro)
- Soporte para múltiples ubicaciones en una request
- Pronósticos extendidos (7-14 días)
- Alertas meteorológicas
- Caché distribuido con Redis

---

## 🛠️ Troubleshooting

### Error: "The requested API version is not supported"

**Causa**: Cliente solicita una versión que no existe

**Solución**: Verificar que la versión solicitada esté definida en el controller con `[ApiVersion("X.Y")]`

### Swagger no muestra la versión en la URL

**Causa**: `SubstituteApiVersionInUrl = false`

**Solución**: Ya está configurado correctamente en `ApiVersioningConfiguration.cs`

### No aparece el dropdown de versiones en Swagger

**Causa**: Solo hay una versión definida

**Solución**: Esto es normal. El dropdown aparece cuando hay múltiples versiones.

---

## 📚 Referencias

- [ASP.NET API Versioning](https://github.com/dotnet/aspnet-api-versioning)
- [REST API Versioning](https://restfulapi.net/versioning/)
- [Semantic Versioning](https://semver.org/)
- [Microsoft Docs - API Versioning](https://learn.microsoft.com/en-us/aspnet/core/web-api/api-versioning)

---

## ✅ Checklist de Implementación

- [x] Instalar paquetes `Asp.Versioning.Http` y `Asp.Versioning.Mvc.ApiExplorer`
- [x] Crear `ApiVersioningConfiguration.cs`
- [x] Crear `SwaggerConfiguration.cs`
- [x] Actualizar `Program.cs` para usar configuraciones modulares
- [x] Actualizar controllers con `[ApiVersion]` y route template
- [x] Configurar Swagger UI para múltiples versiones
- [x] Documentar el versionado
- [ ] Crear tests para diferentes versiones
- [ ] Documentar política de deprecación

---

**Fecha de implementación**: 2025-12-26  
**Versión actual**: v1.0  
**Framework**: .NET 8  
**Librería**: Asp.Versioning 8.1.0

