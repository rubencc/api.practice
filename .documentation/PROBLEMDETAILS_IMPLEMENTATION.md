# ProblemDetails con IExceptionHandler - Documentación

## 📋 Implementación

Se ha implementado el manejo global de excepciones usando **`IExceptionHandler`** de .NET 8, que es el enfoque recomendado por Microsoft para manejar errores de forma centralizada y devolver respuestas en formato **ProblemDetails** (RFC 7807).

**Características clave**:
- ✅ Compatible con **OpenTelemetry** usando `Activity.Current.TraceId`
- ✅ Errores de validación devuelven **422 Unprocessable Entity** (estándar HTTP)
- ✅ Respuestas estandarizadas según RFC 7807

---

## 🏗️ Arquitectura

### Componentes Implementados

```
Api.Weather.Host/
├── ExceptionHandlers/
│   ├── GlobalExceptionHandler.cs      # Handler principal usando IExceptionHandler
│   ├── ValidationException.cs         # Excepción personalizada para validaciones
│   └── NotFoundException.cs           # Excepción personalizada para recursos no encontrados
└── Program.cs                         # Configuración de ProblemDetails y ExceptionHandler
```

---

## 🔧 Configuración

### Program.cs

```csharp
// Registrar ProblemDetails y el ExceptionHandler
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

var app = builder.Build();

// Activar el middleware de ExceptionHandler
app.UseExceptionHandler();
```

**Nota**: `UseExceptionHandler()` debe ir antes de otros middlewares para capturar todas las excepciones.

---

## 📝 Excepciones Personalizadas

### ValidationException

```csharp
namespace Api.Weather.Host.ExceptionHandlers;

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
    
    public ValidationException(string message, Exception innerException) 
        : base(message, innerException) { }
}
```

**Uso**:
```csharp
if (!validationResult)
    throw new ValidationException("Invalid forecast request. Please check location and time fields.");
```

**Respuesta** (422 Unprocessable Entity):
```json
{
  "type": "https://httpstatuses.com/422",
  "title": "Validation Error",
  "status": 422,
  "detail": "Invalid forecast request. Please check location and time fields.",
  "instance": "/api/Weather",
  "traceId": "00-abc123..."
}
```

### NotFoundException

```csharp
namespace Api.Weather.Host.ExceptionHandlers;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
    
    public NotFoundException(string message, Exception innerException) 
        : base(message, innerException) { }
}
```

**Uso**:
```csharp
if (location == null)
    throw new NotFoundException($"Location '{request.Location}' not found.");
```

**Respuesta** (404 Not Found):
```json
{
  "type": "https://httpstatuses.com/404",
  "title": "Resource Not Found",
  "status": 404,
  "detail": "Location 'XYZ' not found.",
  "instance": "/api/Weather",
  "traceId": "00-def456..."
}
```

---

## 🎯 GlobalExceptionHandler

### Excepciones Manejadas

| Excepción | Status Code | Title |
|-----------|-------------|-------|
| `ValidationException` | 422 | Validation Error |
| `NotFoundException` | 404 | Resource Not Found |
| `InvalidOperationException` | 400 | Invalid Operation |
| `ArgumentNullException` | 400 | Argument Null |
| `ArgumentException` | 400 | Invalid Argument |
| Otras excepciones | 500 | Internal Server Error |

### Características

1. **Logging automático**: Todas las excepciones se registran con `ILogger`
2. **Formato ProblemDetails**: Respuestas estandarizadas según RFC 7807
3. **TraceId compatible con OpenTelemetry**: Usa `Activity.Current.TraceId` para correlación distribuida
4. **Tipo de error**: URL al código de estado HTTP
5. **Instancia**: Path del request que causó el error
6. **Código 422 para validaciones**: Siguiendo el estándar HTTP para errores de procesamiento

---

## 🔍 Ejemplo de Uso en Controlador

### Antes (sin manejo global)

```csharp
[HttpPost]
public async Task<IActionResult> GetForecast([FromBody] ForecastRequest request)
{
    try
    {
        if (request == null)
            return BadRequest("Request is null");
            
        var result = await _service.GetForecast(request);
        
        if (result == null)
            return NotFound("Forecast not found");
            
        return Ok(result);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting forecast");
        return StatusCode(500, "Internal server error");
    }
}
```

### Después (con IExceptionHandler)

```csharp
[HttpPost]
[ProducesResponseType(typeof(ForecastResponse), StatusCodes.Status200OK)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status422UnprocessableEntity)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
public async Task<IActionResult> GetForecast([FromBody] ForecastRequest request)
{
    // Validación - lanza excepción si falla
    if (request == null)
        throw new ArgumentNullException(nameof(request), "Request cannot be null");
    
    if (!validationResult)
        throw new ValidationException("Invalid forecast request.");
    
    var locationInfo = await _geolocationService.GetCoordinates(request.Location);
    
    if (locationInfo == (null, null))
        throw new NotFoundException($"Location '{request.Location}' not found.");
    
    // La lógica de negocio continúa sin try-catch
    var forecast = await _weatherQueryService.GetForecastAsync(locationInfo);
    
    return Ok(forecast);
}
```

**Beneficios**:
- ✅ Código más limpio sin try-catch
- ✅ Respuestas consistentes en toda la API
- ✅ Logging centralizado
- ✅ Mejor experiencia de desarrollo

---

## 📊 Ejemplos de Respuestas

### 1. Validación Fallida (422)

**Request**:
```http
POST /api/Weather
Content-Type: application/json

{
  "location": "",
  "time": "2025-12-26T10:00:00Z"
}
```

**Response**:
```json
{
  "type": "https://httpstatuses.com/422",
  "title": "Validation Error",
  "status": 422,
  "detail": "Invalid forecast request. Please check location and time fields.",
  "instance": "/api/Weather",
  "traceId": "00-1234567890abcdef-0123456789abcdef-00"
}
```

### 2. Recurso No Encontrado (404)

**Request**:
```http
POST /api/Weather
Content-Type: application/json

{
  "location": "Atlantis City",
  "time": "2025-12-26T10:00:00Z"
}
```

**Response**:
```json
{
  "type": "https://httpstatuses.com/404",
  "title": "Resource Not Found",
  "status": 404,
  "detail": "Location 'Atlantis City' not found. Please verify the address.",
  "instance": "/api/Weather",
  "traceId": "00-fedcba0987654321-fedcba0987654321-00"
}
```

### 3. Argumento Nulo (400)

**Causa**: `throw new ArgumentNullException(nameof(request))`

**Response**:
```json
{
  "type": "https://httpstatuses.com/400",
  "title": "Argument Null",
  "status": 400,
  "detail": "Required argument 'request' is null",
  "instance": "/api/Weather",
  "traceId": "00-aabbccdd11223344-aabbccdd11223344-00"
}
```

### 4. Error Interno (500)

**Causa**: Cualquier excepción no controlada

**Response**:
```json
{
  "type": "https://httpstatuses.com/500",
  "title": "Internal Server Error",
  "status": 500,
  "detail": "An unexpected error occurred. Please try again later.",
  "instance": "/api/Weather",
  "traceId": "00-99887766554433221-99887766554433221-00"
}
```

**Nota**: En producción, los detalles del error interno se ocultan por seguridad.

---

## 🧪 Pruebas

### Con curl

#### Test 1: Validación fallida
```bash
curl -X POST https://localhost:5001/api/Weather \
  -H "Content-Type: application/json" \
  -d '{"location":"","time":"2025-12-26T10:00:00Z"}' \
  -k -i
```

#### Test 2: Ubicación no encontrada
```bash
curl -X POST https://localhost:5001/api/Weather \
  -H "Content-Type: application/json" \
  -d '{"location":"INVALID_LOCATION_XYZ","time":"2025-12-26T10:00:00Z"}' \
  -k -i
```

#### Test 3: Request exitoso
```bash
curl -X POST https://localhost:5001/api/Weather \
  -H "Content-Type: application/json" \
  -d '{"location":"Madrid, España","time":"2025-12-26T10:00:00Z"}' \
  -k -i
```

### Con Swagger

1. Navega a: https://localhost:5001/swagger
2. Expande el endpoint `POST /api/Weather`
3. Haz clic en "Try it out"
4. Prueba diferentes escenarios:
   - Location vacío → 422 Validation Error
   - Location inválido → 404 Not Found
   - Location válido → 200 OK

---

## 🎁 Ventajas de IExceptionHandler

### vs Middleware Personalizado

| Característica | IExceptionHandler | Middleware Custom |
|----------------|-------------------|-------------------|
| **Integración .NET** | ✅ Nativa | ⚠️ Manual |
| **DI Support** | ✅ Completo | ⚠️ Limitado |
| **Logging** | ✅ Integrado | ❌ Manual |
| **ProblemDetails** | ✅ Automático | ❌ Manual |
| **Múltiples Handlers** | ✅ Sí | ❌ No |
| **Orden de ejecución** | ✅ Controlable | ⚠️ Fijo |
| **Testing** | ✅ Fácil | ⚠️ Complejo |

### vs try-catch en Controllers

| Aspecto | IExceptionHandler | try-catch |
|---------|-------------------|-----------|
| **Código duplicado** | ✅ Eliminado | ❌ En cada método |
| **Consistencia** | ✅ Garantizada | ⚠️ Variable |
| **Mantenibilidad** | ✅ Alta | ❌ Baja |
| **Logging** | ✅ Centralizado | ❌ Disperso |
| **Testing** | ✅ Una vez | ❌ Múltiples veces |

---

## 🔒 Consideraciones de Seguridad

### En Producción

1. **Ocultar detalles internos**:
```csharp
var detail = exception switch
{
    _ when statusCode == 500 => 
        "An unexpected error occurred. Please try again later.",
    _ => exception.Message
};
```

2. **No exponer stack traces** en respuestas HTTP (solo en logs)

3. **Validar datos sensibles** antes de incluirlos en mensajes de error

4. **Rate limiting** para prevenir ataques de fuerza bruta

### Logging

```csharp
_logger.LogError(
    exception,
    "Exception occurred: {Message}",
    exception.Message);
```

- ✅ Incluye el trace ID para correlación
- ✅ Registra la excepción completa (stack trace en logs)
- ✅ No expone información sensible al cliente

---

## 📚 Referencias

- [RFC 7807 - Problem Details for HTTP APIs](https://www.rfc-editor.org/rfc/rfc7807)
- [ASP.NET Core Error Handling](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling)
- [IExceptionHandler Interface](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.diagnostics.iexceptionhandler)
- [ProblemDetails Class](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.problemdetails)

---

## ✅ Checklist de Implementación

- [x] Crear `GlobalExceptionHandler` implementando `IExceptionHandler`
- [x] Crear excepciones personalizadas (`ValidationException`, `NotFoundException`)
- [x] Registrar `AddProblemDetails()` y `AddExceptionHandler<T>()` en DI
- [x] Usar `app.UseExceptionHandler()` en el pipeline
- [x] Actualizar controladores para lanzar excepciones en lugar de retornar IActionResult
- [x] Documentar tipos de respuesta con `[ProducesResponseType]`
- [ ] Agregar tests unitarios para el exception handler
- [ ] Agregar tests de integración para diferentes escenarios de error

---

**Fecha de implementación**: 2025-12-26  
**Versión**: .NET 8  
**Estándar**: RFC 7807 (ProblemDetails)

