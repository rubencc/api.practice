# Open-Meteo Weather Service

## Descripción

`WeatherQueryService` es un servicio que utiliza **Open-Meteo API** para obtener datos meteorológicos en tiempo real a partir de coordenadas geográficas (latitud y longitud).

---

## ✨ Características de Open-Meteo

- ✅ **Totalmente gratuito**: Sin límites de peticiones
- ✅ **Sin API Key**: No requiere registro ni autenticación
- ✅ **Open Source**: Código abierto
- ✅ **Alta disponibilidad**: 99.9% uptime
- ✅ **Rápido**: Respuestas en milisegundos
- ✅ **Datos precisos**: Basado en modelos meteorológicos globales
- 📚 **Documentación**: https://open-meteo.com/en/docs

---

## 🔧 Implementación

### Interfaz

```csharp
public interface IWeatherQueryService : IDisposable
{
    Task<ForecastDto> GetForecastAsync(
        (string latitude, string longitude) info, 
        CancellationToken cancellationToken = default);
        
    Task<IEnumerable<ForecastDto>> GetHistoricalForecastsAsync(
        string location, 
        CancellationToken cancellationToken = default);
}
```

### Uso Básico

```csharp
public class WeatherController : ControllerBase
{
    private readonly IGeolocationService _geolocationService;
    private readonly IWeatherQueryService _weatherService;

    public WeatherController(
        IGeolocationService geolocationService,
        IWeatherQueryService weatherService)
    {
        _geolocationService = geolocationService;
        _weatherService = weatherService;
    }

    [HttpGet("weather/{postalCode}")]
    public async Task<IActionResult> GetWeather(string postalCode)
    {
        // 1. Obtener coordenadas del código postal
        var (lat, lon) = _geolocationService.GetCoordinates(postalCode);
        
        // 2. Obtener clima usando las coordenadas
        var forecast = await _weatherService.GetForecastAsync((lat, lon));
        
        return Ok(forecast);
    }
}
```

---

## 📊 Datos Disponibles

### ForecastDto

```csharp
public class ForecastDto
{
    public string Latitude { get; set; }           // Latitud
    public string Longitude { get; set; }          // Longitud
    public DateTime Time { get; set; }             // Fecha y hora actual
    public double Temperature { get; set; }        // Temperatura en °C
    public double WindSpeed { get; set; }          // Velocidad del viento en km/h
    public int WindDirection { get; set; }         // Dirección del viento en grados (0-360)
    public int WeatherCode { get; set; }           // Código WMO del clima
    public string WeatherDescription { get; set; } // Descripción en español
}
```

### Ejemplo de Respuesta

```json
{
  "latitude": "40.4168",
  "longitude": "-3.7038",
  "time": "2025-12-23T14:30:00",
  "temperature": 15.5,
  "windSpeed": 12.3,
  "windDirection": 270,
  "weatherCode": 2,
  "weatherDescription": "Parcialmente nublado"
}
```

---

## 🌦️ Códigos de Clima (WMO Weather Code)

El servicio traduce automáticamente los códigos WMO a descripciones en español:

| Código | Descripción |
|--------|-------------|
| 0 | Despejado |
| 1 | Principalmente despejado |
| 2 | Parcialmente nublado |
| 3 | Nublado |
| 45, 48 | Niebla |
| 51-57 | Llovizna (ligera, moderada, densa) |
| 61-67 | Lluvia (ligera, moderada, intensa) |
| 71-77 | Nieve |
| 80-82 | Chubascos |
| 85-86 | Chubascos de nieve |
| 95-99 | Tormenta (con o sin granizo) |

Referencia completa: https://open-meteo.com/en/docs

---

## 🔗 API Endpoint

```
GET https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current_weather=true&timezone=auto
```

### Parámetros Usados

- `latitude`: Latitud en grados decimales
- `longitude`: Longitud en grados decimales
- `current_weather=true`: Incluir datos del clima actual
- `timezone=auto`: Ajustar automáticamente a la zona horaria local

### Respuesta de la API

```json
{
  "latitude": 40.4,
  "longitude": -3.7,
  "current_weather": {
    "temperature": 15.5,
    "windspeed": 12.3,
    "winddirection": 270,
    "weathercode": 2,
    "time": "2025-12-23T14:00"
  }
}
```

---

## 🚀 Configuración

### HttpClient (ya configurado)

El servicio está configurado en `HttpClientExtensions.cs`:

```csharp
services.AddHttpClient<IWeatherQueryService, WeatherQueryService>(client =>
{
    client.BaseAddress = new Uri("https://api.open-meteo.com/v1/");
    client.Timeout = TimeSpan.FromSeconds(30);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
})
.AddStandardResilienceHandler(options =>
{
    options.Retry.MaxRetryAttempts = 3;
    options.Retry.BackoffType = Polly.DelayBackoffType.Exponential;
    // ... Circuit Breaker, Timeouts, etc.
});
```

### Políticas de Resiliencia

- ✅ **Retry**: 3 intentos con backoff exponencial
- ✅ **Circuit Breaker**: Protección contra cascadas de fallos
- ✅ **Timeout**: 10 segundos por intento, 30 segundos total

---

## 📈 Ventajas vs Otras APIs

| Característica | Open-Meteo | OpenWeatherMap | Google Weather |
|----------------|------------|----------------|----------------|
| **Gratuito** | ✅ Ilimitado | ⚠️ 1,000/día | ❌ No gratuito |
| **API Key** | ❌ No requiere | ✅ Requiere | ✅ Requiere |
| **Rate Limit** | ✅ Sin límite | ⚠️ 60/min | ⚠️ Limitado |
| **Precisión** | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| **Setup** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐ |
| **Datos** | Completos | Muy completos | Muy completos |

---

## 🎯 Casos de Uso

### 1. Obtener Clima Actual

```csharp
var forecast = await _weatherService.GetForecastAsync(("40.4168", "-3.7038"));
Console.WriteLine($"Temperatura: {forecast.Temperature}°C");
Console.WriteLine($"Clima: {forecast.WeatherDescription}");
```

### 2. Flujo Completo: Código Postal → Clima

```csharp
// Paso 1: Código postal → Coordenadas (OpenCage)
var (lat, lon) = _geolocationService.GetCoordinates("28001");

// Paso 2: Coordenadas → Clima (Open-Meteo)
var forecast = await _weatherService.GetForecastAsync((lat, lon));

// Paso 3: Persistir en MongoDB
await _forecastRepository.SaveAsync(forecast);
```

### 3. Con Caché para Optimización

```csharp
var cacheKey = $"weather_{lat}_{lon}";
if (!_cache.TryGetValue(cacheKey, out ForecastDto forecast))
{
    forecast = await _weatherService.GetForecastAsync((lat, lon));
    _cache.Set(cacheKey, forecast, TimeSpan.FromMinutes(15));
}
```

---

## ⚡ Optimizaciones

### 1. Caché de Resultados

Cachear datos meteorológicos por 15-30 minutos:

```csharp
services.AddMemoryCache();

// Cachear por coordenadas + tiempo
var cacheKey = $"weather_{latitude}_{longitude}_{DateTime.UtcNow:yyyyMMddHH}";
```

### 2. Parámetros Adicionales

Open-Meteo soporta muchos más parámetros:

```
?latitude=40.4&longitude=-3.7
&current_weather=true
&hourly=temperature_2m,precipitation  // Pronóstico por hora
&daily=temperature_2m_max,temperature_2m_min  // Pronóstico diario
&timezone=Europe/Madrid
```

### 3. Batch Requests

Para múltiples ubicaciones, considera agrupar peticiones.

---

## 🐛 Manejo de Errores

### Excepciones

```csharp
try
{
    var forecast = await _weatherService.GetForecastAsync((lat, lon));
}
catch (HttpRequestException ex)
{
    // Error de red o API no disponible
    _logger.LogError(ex, "Error al conectar con Open-Meteo");
    return StatusCode(503, "Servicio meteorológico no disponible");
}
catch (InvalidOperationException ex)
{
    // No se encontraron datos para las coordenadas
    _logger.LogWarning(ex, "No hay datos para {Lat}, {Lon}", lat, lon);
    return NotFound("No se encontraron datos meteorológicos");
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error inesperado en servicio meteorológico");
    return StatusCode(500, "Error interno del servidor");
}
```

---

## 📝 Testing

### Unit Test con NSubstitute

```csharp
[Fact]
public async Task GetForecastAsync_ShouldReturnForecast()
{
    // Arrange
    var weatherService = Substitute.For<IWeatherQueryService>();
    weatherService
        .GetForecastAsync(("40.4168", "-3.7038"), Arg.Any<CancellationToken>())
        .Returns(new ForecastDto
        {
            Temperature = 15.5,
            WeatherDescription = "Parcialmente nublado"
        });

    // Act
    var result = await weatherService.GetForecastAsync(("40.4168", "-3.7038"));

    // Assert
    result.Should().NotBeNull();
    result.Temperature.Should().Be(15.5);
    result.WeatherDescription.Should().Be("Parcialmente nublado");
}
```

### Integration Test

```csharp
[Fact]
public async Task GetForecastAsync_WithRealAPI_ShouldWork()
{
    // Arrange
    var httpClient = new HttpClient();
    var weatherService = new WeatherQueryService(httpClient);

    // Act
    var result = await weatherService.GetForecastAsync(("40.4168", "-3.7038"));

    // Assert
    result.Should().NotBeNull();
    result.Temperature.Should().BeInRange(-50, 50);
    result.WeatherDescription.Should().NotBeEmpty();
}
```

---

## 📚 Recursos

- 🌐 **Sitio web**: https://open-meteo.com/
- 📖 **Documentación API**: https://open-meteo.com/en/docs
- 🔧 **API Playground**: https://open-meteo.com/en/docs#api_form
- 💻 **GitHub**: https://github.com/open-meteo/open-meteo
- 📊 **Dashboard de Estado**: https://status.open-meteo.com/

---

## 🆚 Comparación con Flujo Anterior

### Antes (sin Open-Meteo)

```
PostalCode → ??? → Clima (sin implementar)
```

### Ahora (con Open-Meteo)

```
PostalCode → OpenCage (Geolocation) → Open-Meteo (Weather) → MongoDB
    ↓              ↓                        ↓                    ↓
  28001    (40.4168, -3.7038)     {temp: 15.5°C, ...}      Persistido
```

---

**Fecha de implementación**: 2025-12-23  
**Versión**: 1.0  
**Autor**: Sistema DDD Weather API

