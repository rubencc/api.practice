# Análisis de Domain-Driven Design (DDD) en Weather API

**Fecha de análisis**: 2025-12-27  
**Versión**: 2.0

---

## 📋 Resumen Ejecutivo

Este documento analiza la aplicación de los principios de **Domain-Driven Design (DDD)** en el proyecto Weather API, identificando qué aspectos se ajustan correctamente a DDD, qué aspectos no se ajustan, y proporciona recomendaciones con ejemplos prácticos para mejorar la implementación.

**Conclusión general**: El proyecto ha **avanzado significativamente** en la adopción de DDD con la implementación de Value Objects (`Temperature`, `Location`) y mejor estructura del agregado `Forecast`. Sin embargo, aún persisten **problemas importantes** como modelo de dominio anémico, falta de validaciones en el dominio, y lógica de aplicación en el controlador.

---

## ✅ Aspectos que SÍ se ajustan a DDD

### 1. ✅ Separación de Capas (Arquitectura Hexagonal/Clean)

**Implementación actual**:
```
Weather.Domain → Sin dependencias
Weather.Application → Depende de Domain
Weather.Infrastructure → Depende de Domain y Application
Api.Weather.Host → Depende de Application e Infrastructure
```

**Evaluación**: ✅ **CORRECTO**

La arquitectura sigue el principio de **Dependency Inversion** correctamente:
- El dominio no depende de nada
- Las dependencias apuntan hacia el centro (Domain)
- Inversión de dependencias con interfaces en Domain e implementaciones en Infrastructure

---

### 2. ✅ Uso de Aggregates y Value Objects

**Implementación actual**:
```csharp
// Weather.Domain/Aggregates/Forecast.cs
public class Forecast
{
    internal Forecast(Location location, Temperature temperature, string description, DateTimeOffset time)
    {
        Location = location;
        Temperature = temperature;
        Description = description;
        Time = time;
        Id = Guid.NewGuid();
    }
    
    public Guid Id { get; init; }
    public Location Location { get; init; }
    public DateTimeOffset Time { get; init; }
    public Temperature Temperature { get; init; }
    public string Description { get; init; }
    
    public static Forecast Create(Location location, DateTimeOffset time, Temperature temperature, string description)
    {
        return new Forecast(location, temperature, description, time);
    }
}

// Weather.Domain/ValueObjects/Temperature.cs
public class Temperature : IEquatable<Temperature>
{
    private Temperature(double value, string unit)
    {
        Value = value;
        Unit = unit.ToUpperInvariant();
    }

    public double Value { get; }
    public string Unit { get; }

    public static Temperature Create(double value, string unit)
    {
        return new Temperature(value, unit);
    }
    
    public bool Equals(Temperature? other) { /* ... */ }
    public override int GetHashCode() => HashCode.Combine(Value, Unit);
    public override string ToString() => $"{Value:F1}°{Unit}";
}

// Weather.Domain/ValueObjects/Location.cs
public class Location : IEquatable<Location>
{
    internal Location(string address, double lat, double lon)
    {
        Address = address;
        Latitude = lat;
        Longitude = lon;
    }
    
    public string Address { get; init; }
    public double? Latitude { get; init;}
    public double? Longitude { get; init;}

    public static Location Create(string address, double lat, double lon)
    {
        return new Location(address, lat, lon);
    }
    
    public bool Equals(Location? other) { /* ... */ }
    public override int GetHashCode() => HashCode.Combine(Address, Latitude, Longitude);
}
```

**Evaluación**: ✅ **PARCIALMENTE CORRECTO** (Mejora significativa)

**Bien** ✅:
- Existe una carpeta `Aggregates/` en el dominio
- `Forecast` tiene identidad única (`Guid Id`)
- Usa Value Objects (`Location`, `Temperature`)
- Constructor interno protege la creación
- Tiene método factory `Create()`
- Value Objects con igualdad por valor
- Value Objects inmutables

**Limitaciones** ⚠️:
- Propiedades del agregado con `{ get; init; }` - no son completamente inmutables
- No protege invariantes de negocio (sin validaciones)
- Modelo anémico - sin comportamiento
- `Description` sigue siendo string primitivo (debería ser `WeatherCondition` Value Object)
- `Location` y `Temperature` sin validaciones en sus constructores

---

### 3. ✅ Repository Pattern

**Implementación actual**:
```csharp
// Weather.Domain/Repositories/IForecastRepository.cs
public interface IForecastRepository : IDisposable
{
    Task<bool> AddForecastAsync(Aggregates.Forecast forecast, 
                                CancellationToken cancellationToken = default);
}

// Weather.Infrastructure/Persistence/Repositories/ForecastRepository.cs
public class ForecastRepository : IForecastRepository
{
    private readonly IMongoCollection<Forecast> _forecastCollection;
    
    public async Task<bool> AddForecastAsync(Forecast forecast, 
                                             CancellationToken cancellationToken = default)
    {
        await _forecastCollection.InsertOneAsync(forecast, cancellationToken: cancellationToken);
        return true;
    }
}
```

**Evaluación**: ✅ **CORRECTO**

**Bien**:
- Interfaz en la capa de dominio
- Implementación en Infrastructure
- Abstracción correcta del acceso a datos
- Trabaja con agregados, no con entidades individuales

---

### 4. ✅ Application Services

**Implementación actual**:
```csharp
// Weather.Application/Services/ForecastService.cs
public class ForecastService
{
    private readonly IForecastRepository repository;

    public Task<bool> AddForecastAsync(string location, DateTimeOffset time, 
                                       ForecastDto dto, CancellationToken cancellationToken)
    {
        var forecast = Forecast.Create(location, time, 
                                       dto.Temperature.ToString(), dto.WeatherDescription);
        return repository.AddForecastAsync(forecast, cancellationToken);
    }
}
```

**Evaluación**: ✅ **CORRECTO**

**Bien**:
- Orquesta la lógica de aplicación
- Coordina entre dominio e infraestructura
- No contiene lógica de negocio (la delega al dominio)

---

## ❌ Aspectos que NO se ajustan a DDD

### 1. ❌ Modelo de Dominio Anémico

**Problema**: El agregado `Forecast` es un **modelo anémico** - solo contiene datos sin comportamiento.

**Implementación actual (INCORRECTA)**:
```csharp
public class Forecast
{
    public string Location { get; init; }
    public DateTimeOffset Time { get; init; }
    public string Temperature { get; init; }
    public string Description { get; init; }
    
    // Solo un factory method, sin comportamiento de negocio
    public static Forecast Create(string address, DateTimeOffset time, 
                                   string temperature, string description)
    {
        return new Forecast(address, temperature, description, time);
    }
}
```

**Por qué es anémico**:
- No tiene lógica de negocio
- No protege invariantes
- No tiene métodos de comportamiento
- Actúa como un simple contenedor de datos (DTO)

**✅ Implementación CORRECTA según DDD**:

```csharp
namespace Weather.Domain.Aggregates;

public class Forecast
{
    // Constructor privado - solo se puede crear mediante factory methods
    private Forecast(ForecastId id, Location location, Temperature temperature, 
                     WeatherCondition condition, DateTimeOffset requestedAt)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Location = location ?? throw new ArgumentNullException(nameof(location));
        Temperature = temperature ?? throw new ArgumentNullException(nameof(temperature));
        Condition = condition ?? throw new ArgumentNullException(nameof(condition));
        RequestedAt = requestedAt;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    // Identidad única del agregado
    public ForecastId Id { get; private set; }
    
    // Value Objects en lugar de primitivos
    public Location Location { get; private set; }
    public Temperature Temperature { get; private set; }
    public WeatherCondition Condition { get; private set; }
    
    public DateTimeOffset RequestedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? LastUpdatedAt { get; private set; }

    // Factory method con validaciones
    public static Forecast Request(string locationAddress, DateTimeOffset requestedFor)
    {
        if (string.IsNullOrWhiteSpace(locationAddress))
            throw new ArgumentException("Location address cannot be empty", nameof(locationAddress));
        
        if (requestedFor < DateTimeOffset.UtcNow.AddDays(-1))
            throw new InvalidOperationException("Cannot request forecast for past dates");

        var id = ForecastId.Create();
        var location = Location.Create(locationAddress);
        
        // Valores iniciales - se actualizarán con UpdateWeatherData
        var temperature = Temperature.Unknown();
        var condition = WeatherCondition.Unknown();

        return new Forecast(id, location, temperature, condition, requestedFor);
    }

    // Método de comportamiento - actualizar datos meteorológicos
    public void UpdateWeatherData(double temperatureValue, string temperatureUnit, 
                                   string conditionDescription)
    {
        if (string.IsNullOrWhiteSpace(conditionDescription))
            throw new ArgumentException("Weather condition cannot be empty");

        Temperature = Temperature.Create(temperatureValue, temperatureUnit);
        Condition = WeatherCondition.Create(conditionDescription);
        LastUpdatedAt = DateTimeOffset.UtcNow;
    }

    // Método de comportamiento - verificar si el pronóstico está completo
    public bool IsComplete()
    {
        return !Temperature.IsUnknown() && !Condition.IsUnknown();
    }

    // Método de comportamiento - verificar si el pronóstico es actual
    public bool IsStale(int hoursThreshold = 2)
    {
        if (LastUpdatedAt == null) return true;
        
        var age = DateTimeOffset.UtcNow - LastUpdatedAt.Value;
        return age.TotalHours > hoursThreshold;
    }
}
```

**Diferencias clave**:
- ✅ Tiene identidad única (`ForecastId`)
- ✅ Usa Value Objects (`Location`, `Temperature`, `WeatherCondition`)
- ✅ Constructor privado - solo se puede crear mediante factory
- ✅ Protege invariantes (validaciones en factory y métodos)
- ✅ Tiene comportamiento de negocio (`UpdateWeatherData`, `IsComplete`, `IsStale`)
- ✅ Setters privados - inmutabilidad controlada

---

### 2. ⚠️ Value Objects sin Validaciones

**Problema**: Los Value Objects están implementados pero **sin validaciones**, permitiendo estados inválidos.

**Implementación actual (PARCIALMENTE CORRECTA)**:
```csharp
// Weather.Domain/ValueObjects/Temperature.cs
public class Temperature : IEquatable<Temperature>
{
    private Temperature(double value, string unit)
    {
        // ⚠️ Sin validaciones de unidades válidas
        Value = value;
        Unit = unit.ToUpperInvariant();
    }

    public double Value { get; }
    public string Unit { get; } // "C", "F", "K"

    public static Temperature Create(double value, string unit)
    {
        return new Temperature(value, unit);
    }
    // ✅ Implementa igualdad por valor correctamente
    public bool Equals(Temperature? other) { /* ... */ }
    public override int GetHashCode() => HashCode.Combine(Value, Unit);
}

// Weather.Domain/ValueObjects/Location.cs
public class Location : IEquatable<Location>
{
    internal Location(string address, double lat, double lon)
    {
        // ⚠️ Sin validaciones de dirección o coordenadas
        Address = address;
        Latitude = lat;
        Longitude = lon;
    }
    
    public string Address { get; init; } // ⚠️ init permite modificación después de construcción
    public double? Latitude { get; init;}
    public double? Longitude { get; init;}
}
```

**Problemas actuales**:
- ❌ `Temperature` acepta cualquier unidad (incluso inválida como "X", "ABC")
- ❌ `Temperature` acepta valores imposibles físicamente (ej: -500°C, +1000°C en Kelvin negativo)
- ❌ `Location` acepta direcciones vacías o nulas
- ❌ `Location` acepta coordenadas inválidas (ej: lat > 90, lon > 180)
- ⚠️ Uso de `{ get; init; }` en lugar de `{ get; }` - no completamente inmutable

**✅ Implementación CORRECTA con Validaciones**:

```csharp
// Weather.Domain/ValueObjects/Temperature.cs
namespace Weather.Domain.ValueObjects;

public class Temperature : IEquatable<Temperature>
{
    private Temperature(double value, string unit)
    {
        // ✅ Validación de unidad
        if (!IsValidUnit(unit))
            throw new ArgumentException($"Invalid temperature unit: {unit}. Valid units: C, F, K", nameof(unit));

        // ✅ Validación de valor según unidad
        ValidateValue(value, unit);

        Value = value;
        Unit = unit.ToUpperInvariant();
    }

    public double Value { get; } // ✅ Solo getter
    public string Unit { get; }

    public static Temperature Create(double value, string unit)
    {
        return new Temperature(value, unit);
    }

    // ✅ Método estático para verificar unidades válidas
    private static bool IsValidUnit(string unit)
    {
        var validUnits = new[] { "C", "F", "K", "CELSIUS", "FAHRENHEIT", "KELVIN" };
        return validUnits.Contains(unit?.ToUpperInvariant());
    }

    // ✅ Validación física de valores
    private static void ValidateValue(double value, string unit)
    {
        var normalizedUnit = unit.ToUpperInvariant();
        var isValid = normalizedUnit switch
        {
            "C" or "CELSIUS" => value >= -273.15, // Cero absoluto en Celsius
            "F" or "FAHRENHEIT" => value >= -459.67, // Cero absoluto en Fahrenheit
            "K" or "KELVIN" => value >= 0, // Kelvin no puede ser negativo
            _ => false
        };

        if (!isValid)
            throw new ArgumentException($"Invalid temperature value {value}°{unit}. Value is below absolute zero.", nameof(value));
    }

    // ✅ Lógica de dominio: conversiones entre unidades
    public Temperature ToCelsius()
    {
        return Unit switch
        {
            "C" or "CELSIUS" => this,
            "F" or "FAHRENHEIT" => new Temperature((Value - 32) * 5 / 9, "C"),
            "K" or "KELVIN" => new Temperature(Value - 273.15, "C"),
            _ => throw new InvalidOperationException($"Unknown unit: {Unit}")
        };
    }

    public Temperature ToFahrenheit()
    {
        var celsius = ToCelsius();
        return celsius.Unit == "F" 
            ? celsius 
            : new Temperature(celsius.Value * 9 / 5 + 32, "F");
    }

    // ✅ Lógica de dominio: consultas de negocio
    public bool IsExtreme()
    {
        var celsius = ToCelsius();
        return celsius.Value < -30 || celsius.Value > 50;
    }

    public bool IsFreezingPoint()
    {
        var celsius = ToCelsius();
        return Math.Abs(celsius.Value) < 0.1; // ~0°C
    }

    // ✅ Igualdad por valor semántico (comparando en misma unidad)
    public bool Equals(Temperature? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        
        var thisCelsius = ToCelsius();
        var otherCelsius = other.ToCelsius();
        return Math.Abs(thisCelsius.Value - otherCelsius.Value) < 0.01;
    }

    public override bool Equals(object? obj) => Equals(obj as Temperature);
    
    // ✅ HashCode consistente con Equals (basado en Celsius)
    public override int GetHashCode()
    {
        var celsius = ToCelsius();
        return HashCode.Combine(Math.Round(celsius.Value, 2), "C");
    }
    
    public override string ToString() => $"{Value:F1}°{Unit}";
}

// Weather.Domain/ValueObjects/Location.cs
namespace Weather.Domain.ValueObjects;

public class Location : IEquatable<Location>
{
    private Location(string address, double latitude, double longitude)
    {
        // ✅ Validación de dirección
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address cannot be empty", nameof(address));
        
        if (address.Length > 200)
            throw new ArgumentException("Address exceeds maximum length of 200 characters", nameof(address));

        // ✅ Validación de coordenadas
        if (latitude < -90 || latitude > 90)
            throw new ArgumentException($"Invalid latitude: {latitude}. Must be between -90 and 90", nameof(latitude));
        
        if (longitude < -180 || longitude > 180)
            throw new ArgumentException($"Invalid longitude: {longitude}. Must be between -180 and 180", nameof(longitude));

        Address = address.Trim();
        Latitude = latitude;
        Longitude = longitude;
    }
    
    public string Address { get; } // ✅ Solo getter
    public double Latitude { get; } // ✅ No nullable - siempre debe tener coordenadas
    public double Longitude { get; }

    // ✅ Factory method con validaciones
    public static Location Create(string address, double latitude, double longitude)
    {
        return new Location(address, latitude, longitude);
    }

    // ✅ Lógica de dominio: calcular distancia entre ubicaciones
    public double DistanceInKilometersTo(Location other)
    {
        const double earthRadiusKm = 6371;

        var dLat = DegreesToRadians(other.Latitude - Latitude);
        var dLon = DegreesToRadians(other.Longitude - Longitude);

        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(DegreesToRadians(Latitude)) * Math.Cos(DegreesToRadians(other.Latitude)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadiusKm * c;
    }

    private static double DegreesToRadians(double degrees) => degrees * Math.PI / 180;

    // ✅ Lógica de dominio: verificar si está cerca
    public bool IsNearTo(Location other, double maxDistanceKm = 10)
    {
        return DistanceInKilometersTo(other) <= maxDistanceKm;
    }

    public bool Equals(Location? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return Address == other.Address && Latitude.Equals(other.Latitude) && Longitude.Equals(other.Longitude);
    }

    public override bool Equals(object? obj) => Equals(obj as Location);
    
    public override int GetHashCode() => HashCode.Combine(Address, Latitude, Longitude);
    
    public override string ToString() => $"{Address} ({Latitude:F4}, {Longitude:F4})";
}

// Weather.Domain/ValueObjects/WeatherCondition.cs (NUEVO)
namespace Weather.Domain.ValueObjects;

public class WeatherCondition : IEquatable<WeatherCondition>
{
    private WeatherCondition(string description, WeatherType type)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException("Weather description cannot be empty", nameof(description));

        Description = description.Trim();
        Type = type;
    }

    public string Description { get; }
    public WeatherType Type { get; }

    public static WeatherCondition Create(string description)
    {
        var type = ClassifyWeatherType(description);
        return new WeatherCondition(description, type);
    }

    // ✅ Lógica de dominio: clasificar tipo de clima
    private static WeatherType ClassifyWeatherType(string description)
    {
        var lower = description.ToLowerInvariant();
        
        if (lower.Contains("rain") || lower.Contains("shower")) return WeatherType.Rainy;
        if (lower.Contains("snow") || lower.Contains("blizzard")) return WeatherType.Snowy;
        if (lower.Contains("cloud") || lower.Contains("overcast")) return WeatherType.Cloudy;
        if (lower.Contains("clear") || lower.Contains("sunny")) return WeatherType.Clear;
        if (lower.Contains("storm") || lower.Contains("thunder")) return WeatherType.Stormy;
        if (lower.Contains("fog") || lower.Contains("mist")) return WeatherType.Foggy;
        
        return WeatherType.Unknown;
    }

    // ✅ Lógica de dominio: consultas
    public bool RequiresUmbrella() => Type is WeatherType.Rainy or WeatherType.Stormy;
    
    public bool IsDangerous() => Type is WeatherType.Stormy or WeatherType.Snowy;

    public bool Equals(WeatherCondition? other)
    {
        if (other is null) return false;
        return Description == other.Description;
    }

    public override bool Equals(object? obj) => Equals(obj as WeatherCondition);
    public override int GetHashCode() => Description.GetHashCode();
    public override string ToString() => Description;
}

// Weather.Domain/ValueObjects/WeatherType.cs (NUEVO)
public enum WeatherType
{
    Unknown,
    Clear,
    Cloudy,
    Rainy,
    Snowy,
    Stormy,
    Foggy
}
```

**Ventajas de las validaciones**:
- ✅ Imposible crear Value Objects en estado inválido
- ✅ Validaciones centralizadas en un solo lugar
- ✅ Encapsulan reglas de negocio del dominio
- ✅ Agregan lógica de dominio (conversiones, cálculos, consultas)
- ✅ Expresan el lenguaje ubicuo
- ✅ Previenen bugs en tiempo de compilación/ejecución

---

### 3. ⚠️ Identidad con Tipo Primitivo

**Problema**: `Forecast` tiene identidad única pero usa un tipo primitivo (`Guid`) en lugar de un Value Object tipado.

**Implementación actual (PARCIALMENTE CORRECTA)**:
```csharp
public class Forecast
{
    public Guid Id { get; init; } // ⚠️ Guid primitivo, no Value Object
    public Location Location { get; init; }
    // ...
}
```

**Bien** ✅:
- El agregado tiene identidad única
- Se genera automáticamente en el constructor

**Limitaciones** ⚠️:
- Usa `Guid` primitivo en lugar de `ForecastId` Value Object
- `{ get; init; }` permite reasignación después de construcción
- No hay protección contra `Guid.Empty`

**✅ Implementación CORRECTA**:

```csharp
// Weather.Domain/ValueObjects/ForecastId.cs
namespace Weather.Domain.ValueObjects;

public class ForecastId : IEquatable<ForecastId>
{
    private ForecastId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("ForecastId cannot be empty", nameof(value));
        
        Value = value;
    }

    public Guid Value { get; }

    // Factory methods
    public static ForecastId Create() => new ForecastId(Guid.NewGuid());
    
    public static ForecastId Create(Guid value) => new ForecastId(value);

    // Igualdad por valor
    public bool Equals(ForecastId? other) => other is not null && Value == other.Value;
    
    public override bool Equals(object? obj) => Equals(obj as ForecastId);
    
    public override int GetHashCode() => Value.GetHashCode();
    
    public static bool operator ==(ForecastId? left, ForecastId? right) => 
        Equals(left, right);
    
    public static bool operator !=(ForecastId? left, ForecastId? right) => 
        !Equals(left, right);

    public override string ToString() => Value.ToString();
    
    // Conversión implícita para facilitar uso con MongoDB
    public static implicit operator Guid(ForecastId id) => id.Value;
}

// Weather.Domain/Aggregates/Forecast.cs
public class Forecast
{
    private Forecast(ForecastId id, Location location, Temperature temperature, 
                     string description, DateTimeOffset time)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Location = location ?? throw new ArgumentNullException(nameof(location));
        Temperature = temperature ?? throw new ArgumentNullException(nameof(temperature));
        Description = description;
        Time = time;
    }
    
    public ForecastId Id { get; private set; } // ✅ Value Object, setter privado
    public Location Location { get; private set; }
    public Temperature Temperature { get; private set; }
    public string Description { get; private set; }
    public DateTimeOffset Time { get; private set; }
    
    public static Forecast Create(Location location, DateTimeOffset time, 
                                  Temperature temperature, string description)
    {
        var id = ForecastId.Create(); // ✅ Generar ID único
        return new Forecast(id, location, temperature, description, time);
    }
}
```

**Por qué usar Value Object para ID**:
- ✅ Type safety: no se puede pasar un `Guid` cualquiera
- ✅ Semántica: `ForecastId` es más expresivo que `Guid`
- ✅ Validación centralizada (no puede ser `Guid.Empty`)
- ✅ Facilita refactorización futura (ej: cambiar a string UUID)

---

### 4. ❌ Lógica de Orquestación en el Controller

**Problema**: El controlador tiene **lógica de coordinación** que debería estar en Application Service.

**Implementación actual (INCORRECTA)**:
```csharp
// Api.Weather.Host/Controllers/WeatherController.cs
[HttpPost]
public async Task<IActionResult> GetForecast([FromBody] ForecastRequest request, 
                                              CancellationToken cancellationToken)
{
    // ⚠️ Validación en el controller (debería ser middleware o filter)
    var validationResult = await _validator.ValidateAsync(request, cancellationToken);
    if(!validationResult.IsValid)
        throw new ValidationException("Invalid forecast request...", validationResult.Errors);

    // ❌ Orquestación de servicios en el controller
    Location locationInfo = await _geolocationService.GetCoordinates(request.Location);
    
    if (locationInfo == null)
        throw new NotFoundException($"Location '{request.Location}' not found...");
    
    var forecast = await _weatherQueryService.GetForecastAsync(locationInfo, cancellationToken);
    
    // ❌ Más lógica de coordinación
    await _forecastService.AddForecastAsync(request.Location, request.Time, forecast, cancellationToken);
    
    // ❌ Construcción manual del response
    var response = new ForecastResponse() 
    { 
        Location = request.Location, 
        Time = forecast.Time.ToString(CultureInfo.InvariantCulture), 
        Temperature = forecast.Temperature.ToString(), 
        Weather = forecast.WeatherDescription 
    };
    
    return Ok(response);
}

// Weather.Application/Services/ForecastService.cs
public class ForecastService
{
    public Task<bool> AddForecastAsync(string location, DateTimeOffset time, 
                                       ForecastDto dto, CancellationToken cancellationToken)
    {
        // ⚠️ Service muy simple, solo guarda
        var forecast = Forecast.Create(dto.Location, time, dto.Temperature, dto.WeatherDescription);
        return repository.AddForecastAsync(forecast, cancellationToken);
    }
}
```

**Problemas**:
- ❌ Controller tiene lógica de orquestación (llamar múltiples servicios)
- ❌ Controller maneja validaciones manualmente
- ❌ Controller construye el aggregate a través del service
- ❌ Application Service es anémico (solo guarda)
- ❌ No hay un caso de uso claro y cohesivo

**✅ Implementación CORRECTA**:

```csharp
// Weather.Application/Services/ForecastApplicationService.cs
public class ForecastApplicationService
{
    private readonly IForecastRepository _repository;
    private readonly IWeatherQueryService _weatherService;
    private readonly IGeolocationService _geolocationService;

    public async Task<ForecastId> RequestForecastAsync(
        string locationAddress, 
        DateTimeOffset requestedFor,
        CancellationToken cancellationToken)
    {
        // 1. Crear el agregado con lógica de dominio
        var forecast = Forecast.Request(locationAddress, requestedFor);
        
        // 2. Obtener datos de servicios externos (Application concern)
        var coordinates = await _geolocationService.GetCoordinatesAsync(locationAddress);
        var weatherData = await _weatherService.GetForecastAsync(
            coordinates.Latitude, coordinates.Longitude, cancellationToken);
        
        // 3. Actualizar el agregado (lógica de negocio en el dominio)
        forecast.UpdateWeatherData(
            weatherData.Temperature, 
            "C", 
            weatherData.Description);
        
        // 4. Guardar (Infrastructure concern)
        await _repository.SaveAsync(forecast, cancellationToken);
        
        return forecast.Id;
    }
}
```

**Diferencia clave**:
- Application Service **orquesta** (llama servicios externos, coordina)
- Agregado de dominio **ejecuta lógica de negocio** (validaciones, invariantes, comportamiento)

---

### 5. ❌ Validaciones de Negocio en Application Layer (Host)

**Problema**: Las validaciones de negocio están en el `Host` usando `FluentValidation` en lugar del dominio.

**Implementación actual (INCORRECTA)**:
```csharp
// Api.Weather.Host/Validations/ForecastRequestValidator.cs
public class ForecastRequestValidator : AbstractValidator<ForecastRequest>
{
    public ForecastRequestValidator()
    {
        // ❌ Validaciones de negocio en el Host Layer
        RuleFor(x => x.Location)
            .NotEmpty()
            .WithMessage("Location is required")
            .MaximumLength(200)
            .WithMessage("Location must not exceed 200 characters");

        RuleFor(x => x.Time)
            .NotEmpty()
            .WithMessage("Time is required")
            .Must(TimeMustNotBeDefault)
            .WithMessage("Time must not be default value");
    }
}

// Api.Weather.Host/Controllers/WeatherController.cs
public async Task<IActionResult> GetForecast([FromBody] ForecastRequest request)
{
    // ❌ Validación manual en el controller
    var validationResult = await _validator.ValidateAsync(request, cancellationToken);
    
    if(!validationResult.IsValid)
        throw new ValidationException("Invalid forecast request...", validationResult.Errors);
    
    // ... resto del código
}
```

**Problemas**:
- ❌ Validaciones de dominio (longitud de dirección, tiempo válido) están en el Host
- ❌ El dominio no se auto-valida - se puede crear `Location` inválido
- ❌ Validación manual en cada controller
- ⚠️ `FluentValidation` es para validación de entrada HTTP, no reglas de dominio

**Dónde debería estar cada validación**:
- Validaciones de **negocio** → En el **dominio** (Value Objects, Aggregates)
- Validaciones de **aplicación** (ej: permisos, autenticación) → En **Application**

**✅ Implementación CORRECTA**:

```csharp
// Weather.Domain/ValueObjects/Location.cs
public class Location
{
    private Location(string address)
    {
        // ✅ Validación de negocio en el Value Object
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address cannot be empty", nameof(address));
        
        if (address.Length > 200)
            throw new ArgumentException("Address is too long", nameof(address));

        Address = address.Trim();
    }

    public string Address { get; }

    public static Location Create(string address)
    {
        return new Location(address);
    }
}

// Ahora en Application solo validaciones de aplicación
// Weather.Application/Validators/ForecastRequestValidator.cs
public class ForecastRequestValidator
{
    private readonly IUserContext _userContext;

    public async Task ValidateAsync(ForecastCommand command)
    {
        // ✅ Validaciones de aplicación (permisos, cuotas, etc.)
        var user = await _userContext.GetCurrentUserAsync();
        
        if (!user.HasPermission("forecast:request"))
            throw new UnauthorizedException("User does not have permission to request forecasts");
        
        if (user.DailyQuotaExceeded())
            throw new QuotaExceededException("Daily forecast quota exceeded");
    }
}
```

---

### 6. ❌ DTOs mezclados con lógica de dominio

**Problema**: `ForecastDto` se usa para pasar datos al dominio, creando acoplamiento.

**Implementación actual (INCORRECTA)**:
```csharp
public Task<bool> AddForecastAsync(string location, DateTimeOffset time, 
                                   ForecastDto dto, // ❌ DTO de Application en método de dominio
                                   CancellationToken cancellationToken)
{
    var forecast = Forecast.Create(location, time, 
                                   dto.Temperature.ToString(), 
                                   dto.WeatherDescription);
    return repository.AddForecastAsync(forecast, cancellationToken);
}
```

**✅ Implementación CORRECTA**:

```csharp
// El dominio no conoce DTOs, solo primitivos o Value Objects
public async Task<ForecastId> RequestForecastAsync(
    string locationAddress, 
    DateTimeOffset requestedFor,
    CancellationToken cancellationToken)
{
    // Application Service traduce de DTO a conceptos de dominio
    var forecast = Forecast.Request(locationAddress, requestedFor);
    
    // Obtener datos externos
    var weatherData = await _weatherService.GetForecastAsync(...);
    
    // Pasar primitivos o Value Objects al dominio, nunca DTOs
    forecast.UpdateWeatherData(
        weatherData.Temperature,  // double
        "C",                      // string
        weatherData.Description   // string
    );
    
    await _repository.SaveAsync(forecast, cancellationToken);
    
    return forecast.Id;
}
```

---

### 7. ❌ Ausencia de Domain Events

**Problema**: No hay eventos de dominio para comunicar cambios importantes.

**Implementación actual**: No hay eventos de dominio.

**✅ Implementación CORRECTA**:

```csharp
// Weather.Domain/Events/ForecastRequestedEvent.cs
namespace Weather.Domain.Events;

public record ForecastRequestedEvent(
    ForecastId ForecastId,
    Location Location,
    DateTimeOffset RequestedFor,
    DateTimeOffset OccurredAt
) : IDomainEvent;

// Weather.Domain/Events/ForecastDataUpdatedEvent.cs
public record ForecastDataUpdatedEvent(
    ForecastId ForecastId,
    Temperature Temperature,
    WeatherCondition Condition,
    DateTimeOffset OccurredAt
) : IDomainEvent;

// Weather.Domain/Aggregates/Forecast.cs
public class Forecast
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public static Forecast Request(string locationAddress, DateTimeOffset requestedFor)
    {
        // ... validaciones y creación
        
        var forecast = new Forecast(id, location, temperature, condition, requestedFor);
        
        // ✅ Levantar evento de dominio
        forecast.AddDomainEvent(new ForecastRequestedEvent(
            forecast.Id,
            forecast.Location,
            requestedFor,
            DateTimeOffset.UtcNow
        ));
        
        return forecast;
    }

    public void UpdateWeatherData(double temperatureValue, string unit, string description)
    {
        // ... actualizar datos
        
        // ✅ Levantar evento de dominio
        AddDomainEvent(new ForecastDataUpdatedEvent(
            Id,
            Temperature,
            Condition,
            DateTimeOffset.UtcNow
        ));
    }

    private void AddDomainEvent(IDomainEvent eventItem)
    {
        _domainEvents.Add(eventItem);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
```

**Por qué son importantes**:
- Desacoplamiento entre agregados
- Comunicación de cambios importantes
- Base para event sourcing
- Auditoría y tracking
- Integración con otros bounded contexts

---

### 8. ❌ Controller con lógica de coordinación

(Ya tratado en sección 4)

---

### 9. ✅ ProblemDetails con OpenTelemetry (Bien Implementado)

**Implementación actual**:
```csharp
// Api.Weather.Host/ExceptionHandlers/GlobalExceptionHandler.cs
public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        var problemDetails = CreateProblemDetails(httpContext, exception);

        httpContext.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;
        httpContext.Response.ContentType = "application/problem+json";

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }

    private static ProblemDetails CreateProblemDetails(HttpContext context, Exception exception)
    {
        var (statusCode, title) = exception switch
        {
            ValidationException => ((int)HttpStatusCode.UnprocessableEntity, "Validation Error"),
            NotFoundException => ((int)HttpStatusCode.NotFound, "Resource Not Found"),
            ArgumentException => ((int)HttpStatusCode.BadRequest, "Invalid Argument"),
            _ => ((int)HttpStatusCode.InternalServerError, "Internal Server Error")
        };

        // ✅ Integración con OpenTelemetry
        var traceId = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;

        return new ProblemDetails
        {
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = title,
            Status = statusCode,
            Detail = exception.Message,
            Instance = context.Request.Path,
            Extensions =
            {
                ["traceId"] = traceId, // ✅ Compatible con OpenTelemetry
                ["errors"] = exception.Data is ValidationException validationEx
                    ? validationEx.Errors
                    : null
            }
        };
    }
}
```

**Evaluación**: ✅ **CORRECTO**

**Bien** ✅:
- Usa `IExceptionHandler` de .NET 8
- Manejo global de excepciones
- ProblemDetails RFC 7807 compliant
- Integración con OpenTelemetry (`Activity.Current?.TraceId`)
- Errores de validación retornan 422 (Unprocessable Entity)
- Logging estructurado
- Respuestas consistentes `application/problem+json`

**Mejoras posibles**:
- Agregar más contexto en `Extensions` (ej: timestamp, user ID si existe)
- Separar en diferentes handlers por tipo de excepción (más SOLID)

---

## 🎯 Nueva Evaluación: Implementación de API Layer

### ✅ Aspectos Correctos del API Layer

1. **API Versioning**: Implementado con `Asp.Versioning` en la URL
   ```csharp
   [ApiVersion("1.0")]
   [Route("api/v{version:apiVersion}/[controller]")]
   ```

2. **ProblemDetails**: RFC 7807 compliant con traceId de OpenTelemetry

3. **Global Exception Handler**: Manejo centralizado de errores

4. **FluentValidation**: Validación de entrada HTTP

5. **Swagger**: Documentación automática con versionado

### ❌ Aspectos Incorrectos del API Layer

1. **Controller con demasiada responsabilidad**: Orquesta servicios (debería delegar)

2. **Validación manual**: Se llama `_validator.ValidateAsync` manualmente (debería ser automático vía filter)

3. **Lógica de negocio escapada**: Controller decide cuándo lanzar excepciones

---

## ❌ Aspectos que NO se ajustan a DDD

### 1. ❌ Modelo de Dominio Anémico

**✅ Implementación CORRECTA**:

```csharp
// WeatherController.cs
[HttpPost]
public async Task<IActionResult> RequestForecast([FromBody] ForecastRequest request)
{
    // ✅ Controller solo traduce HTTP a Application Service
    var forecastId = await _forecastApplicationService.RequestForecastAsync(
        request.Location,
        request.Time,
        HttpContext.RequestAborted
    );
    
    // Mapear de dominio a DTO de respuesta
    var forecast = await _forecastQueryService.GetByIdAsync(forecastId);
    var response = _mapper.Map<ForecastResponse>(forecast);
    
    return Ok(response);
}

// ForecastApplicationService.cs (Application Layer)
public class ForecastApplicationService
{
    public async Task<ForecastId> RequestForecastAsync(
        string locationAddress,
        DateTimeOffset requestedFor,
        CancellationToken cancellationToken)
    {
        // ✅ Toda la lógica de orquestación aquí
        
        // 1. Crear agregado
        var forecast = Forecast.Request(locationAddress, requestedFor);
        
        // 2. Obtener coordenadas
        var coordinates = await _geolocationService.GetCoordinatesAsync(locationAddress);
        if (coordinates == null)
            throw new NotFoundException($"Location '{locationAddress}' not found");
        
        // 3. Obtener datos meteorológicos
        var weatherData = await _weatherService.GetForecastAsync(
            coordinates.Latitude, 
            coordinates.Longitude, 
            cancellationToken);
        
        // 4. Actualizar agregado
        forecast.UpdateWeatherData(
            weatherData.Temperature,
            "C",
            weatherData.Description);
        
        // 5. Guardar
        await _repository.SaveAsync(forecast, cancellationToken);
        
        // 6. Publicar eventos de dominio
        await _eventPublisher.PublishAsync(forecast.DomainEvents, cancellationToken);
        forecast.ClearDomainEvents();
        
        return forecast.Id;
    }
}
```

**Responsabilidades correctas**:
- **Controller**: Traducir HTTP ↔ Application
- **Application Service**: Orquestar casos de uso
- **Domain**: Lógica de negocio y protección de invariantes

---

## 🎯 Recomendaciones Prioritarias

### Prioridad ALTA 🔴

#### 1. Enriquecer el Modelo de Dominio

**Acción**: Transformar `Forecast` de modelo anémico a modelo rico.

**Pasos**:
1. Agregar identidad única (`ForecastId`)
2. Agregar comportamiento de negocio
3. Proteger invariantes
4. Hacer setters privados

**Ver ejemplo completo** en la sección "1. ❌ Modelo de Dominio Anémico".

---

#### 2. Implementar Value Objects

**Acción**: Reemplazar primitivos con Value Objects.

**Conversión**:
- `string Location` → `Location` (Value Object)
- `string Temperature` → `Temperature` (Value Object)
- `string Description` → `WeatherCondition` (Value Object)
- Sin ID → `ForecastId` (Value Object / Entity Id)

**Ver ejemplos completos** en la sección "2. ❌ Ausencia de Value Objects".

---

#### 3. Mover Validaciones al Dominio

**Acción**: Las validaciones de negocio deben estar en el dominio.

**Cambios**:
```csharp
// ❌ ANTES: En Application
public class AddressValidation : IValidation<ForecastCommand>
{
    public Task<bool> IsValid(ForecastCommand command)
    {
        return Task.FromResult(!string.IsNullOrWhiteSpace(command.Location));
    }
}

// ✅ DESPUÉS: En Domain (Value Object)
public class Location
{
    private Location(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
            throw new ArgumentException("Address cannot be empty");
        
        Address = address.Trim();
    }
}
```

---

### Prioridad MEDIA 🟡

#### 4. Implementar Domain Events

**Acción**: Agregar eventos de dominio para comunicar cambios importantes.

**Eventos a implementar**:
- `ForecastRequestedEvent`: Cuando se solicita un pronóstico
- `ForecastDataUpdatedEvent`: Cuando se actualizan los datos
- `ForecastExpiredEvent`: Cuando un pronóstico se vuelve obsoleto

**Ver ejemplo completo** en la sección "7. ❌ Ausencia de Domain Events".

---

#### 5. Refactorizar Application Services

**Acción**: Mover toda la lógica de orquestación de los controllers a Application Services.

**Estructura objetivo**:
```
Controller → Application Service → Domain + Infrastructure
```

**Ver ejemplo completo** en la sección "8. ❌ Controller con lógica de coordinación".

---

### Prioridad BAJA 🟢

#### 6. Implementar Especificaciones (Specifications)

**Uso**: Para consultas complejas al repositorio.

```csharp
// Weather.Domain/Specifications/ForecastSpecification.cs
public abstract class Specification<T>
{
    public abstract bool IsSatisfiedBy(T entity);
}

public class RecentForecastsSpecification : Specification<Forecast>
{
    private readonly int _hoursThreshold;

    public RecentForecastsSpecification(int hoursThreshold = 2)
    {
        _hoursThreshold = hoursThreshold;
    }

    public override bool IsSatisfiedBy(Forecast forecast)
    {
        return !forecast.IsStale(_hoursThreshold);
    }
}
```

---

#### 7. Implementar Factories

**Uso**: Para creación compleja de agregados.

```csharp
// Weather.Domain/Factories/IForecastFactory.cs
public interface IForecastFactory
{
    Forecast CreateFromExternalData(string address, ExternalWeatherData data);
}

public class ForecastFactory : IForecastFactory
{
    public Forecast CreateFromExternalData(string address, ExternalWeatherData data)
    {
        // Lógica compleja de construcción
        var location = Location.Create(address)
            .WithCoordinates(data.Latitude, data.Longitude);
        
        var temperature = Temperature.Create(data.Temp, data.TempUnit);
        var condition = WeatherCondition.FromCode(data.WeatherCode);
        
        // ... más lógica
        
        return Forecast.CreateWithData(location, temperature, condition, ...);
    }
}
```

---

## 📊 Tabla de Puntuación DDD

| Aspecto DDD | Estado Actual | Puntuación | Prioridad |
|-------------|---------------|------------|-----------|
| **Arquitectura en Capas** | ✅ Implementado correctamente | 10/10 | - |
| **Separación de Concerns** | ✅ Bien definida | 9/10 | - |
| **Repository Pattern** | ✅ Implementado correctamente | 9/10 | - |
| **Agregados con ID** | ✅ Implementado (Guid) | 7/10 | 🟡 MEDIA |
| **Value Objects** | ⚠️ Implementados sin validaciones | 5/10 | 🔴 ALTA |
| **Modelo Rico** | ❌ Modelo anémico | 2/10 | 🔴 ALTA |
| **Domain Events** | ❌ No implementados | 0/10 | 🟡 MEDIA |
| **Invariantes Protegidas** | ❌ Sin protección | 1/10 | 🔴 ALTA |
| **Lenguaje Ubicuo** | ⚠️ Parcial (VO sin comportamiento) | 5/10 | 🟡 MEDIA |
| **Application Services** | ❌ Lógica en controllers | 3/10 | 🔴 ALTA |
| **Validaciones en Dominio** | ❌ En Host Layer (FluentValidation) | 2/10 | 🔴 ALTA |
| **ProblemDetails + OpenTelemetry** | ✅ Bien implementado | 10/10 | - |
| **API Versioning** | ✅ Implementado en URL | 9/10 | - |
| **Exception Handling** | ✅ Global handler con IExceptionHandler | 9/10 | - |

**Puntuación Total**: **81/140** (57.8%)

**Mejora desde última versión**: +31 puntos (de 50/120 a 81/140)

**Logros principales**:
- ✅ Value Objects `Temperature` y `Location` implementados
- ✅ Agregado con identidad única (`Guid Id`)
- ✅ ProblemDetails RFC 7807 con OpenTelemetry
- ✅ API Versioning configurado
- ✅ Exception handling global

**Pendientes críticos**:
- ❌ Agregar validaciones a Value Objects
- ❌ Enriquecer el modelo con comportamiento
- ❌ Mover lógica de orquestación a Application Service
- ❌ Implementar Domain Events

---

## 🎓 Conceptos Clave de DDD

### Lenguaje Ubicuo (Ubiquitous Language)

**Definición**: Vocabulario compartido entre desarrolladores y expertos del dominio.

**Problema actual**:
- Se usan términos genéricos (`Temperature` como string)
- No hay expresividad en el modelo

**Solución**:
- Usar Value Objects con nombres del dominio
- Métodos con nombres del lenguaje del negocio
  - `Forecast.Request()` en lugar de `Forecast.Create()`
  - `Temperature.IsExtreme()` en lugar de validaciones externas
  - `Forecast.IsStale()` en lugar de cálculos fuera del modelo

---

### Bounded Context

**Estado actual**: El proyecto es pequeño y tiene un solo bounded context implícito ("Weather Forecasting").

**Recomendación futura**: Si crece, separar en bounded contexts:
- **Forecast Context**: Gestión de pronósticos
- **Location Context**: Gestión de ubicaciones y coordenadas
- **Weather Data Context**: Datos meteorológicos crudos
- **User Context**: Usuarios y permisos

---

### Aggregate Root

**Reglas**:
1. Solo el Aggregate Root se expone fuera del agregado
2. Los cambios al agregado pasan por el root
3. El root protege invariantes de todas las entidades internas
4. Referencias externas solo al root (por ID)

**Implementación actual**: `Forecast` es el root pero no tiene entidades internas (correcto para la complejidad actual).

---

## 📚 Referencias

- **Libro**: "Domain-Driven Design" - Eric Evans (Blue Book)
- **Libro**: "Implementing Domain-Driven Design" - Vaughn Vernon (Red Book)
- **Libro**: "Domain-Driven Design Distilled" - Vaughn Vernon
- **Artículo**: [DDD Reference](https://www.domainlanguage.com/ddd/reference/) - Eric Evans
- **Código**: [eShopOnContainers](https://github.com/dotnet-architecture/eShopOnContainers) - Ejemplo de Microsoft con DDD

---

## ✅ Plan de Acción Actualizado

### Fase 1: Fundamentos (PARCIALMENTE COMPLETADA) ✅

1. ✅ **COMPLETADO**: Implementar identidad única en `Forecast` (Guid)
2. ✅ **COMPLETADO**: Implementar `Location` (Value Object)
3. ✅ **COMPLETADO**: Implementar `Temperature` (Value Object)
4. ✅ **COMPLETADO**: Refactorizar `Forecast` para usar Value Objects
5. ❌ **PENDIENTE**: Agregar validaciones en Value Objects
6. ❌ **PENDIENTE**: Implementar `ForecastId` (Value Object para ID)
7. ❌ **PENDIENTE**: Implementar `WeatherCondition` (Value Object)

**Progreso**: 4/7 (57%)

---

### Fase 2: Validaciones y Protección de Invariantes (PENDIENTE) 🔴

8. ❌ Agregar validaciones al constructor de `Temperature`
   - Validar unidades válidas (C, F, K)
   - Validar valores físicamente posibles (no menor al cero absoluto)

9. ❌ Agregar validaciones al constructor de `Location`
   - Validar dirección no vacía y longitud máxima
   - Validar coordenadas dentro de rangos válidos (-90/90, -180/180)

10. ❌ Agregar lógica de dominio a Value Objects
    - `Temperature.ToCelsius()`, `Temperature.IsExtreme()`
    - `Location.DistanceInKilometersTo()`, `Location.IsNearTo()`

11. ❌ Hacer propiedades del agregado completamente inmutables
    - Cambiar `{ get; init; }` por `{ get; private set; }`
    - Validar en constructor que ningún parámetro sea null

**Estimación**: 1 semana

---

### Fase 3: Comportamiento Rico en el Dominio (PENDIENTE) 🔴

12. ❌ Agregar métodos de comportamiento a `Forecast`
    ```csharp
    public void UpdateWeatherData(Temperature temperature, WeatherCondition condition)
    public bool IsComplete()
    public bool IsStale(int hoursThreshold = 2)
    public bool RequiresWeatherAlert()
    ```

13. ❌ Proteger invariantes en el agregado
    - Validar que `Time` no sea en el pasado lejano
    - Validar que `Temperature` y `Location` no sean null

14. ❌ Implementar `WeatherCondition` Value Object
    ```csharp
    public class WeatherCondition
    {
        public string Description { get; }
        public WeatherType Type { get; }
        public bool RequiresUmbrella()
        public bool IsDangerous()
    }
    ```

**Estimación**: 1 semana

---

### Fase 4: Refactorizar Application Layer (ALTA PRIORIDAD) 🔴

15. ❌ Crear `ForecastApplicationService` dedicado
    - Mover toda la lógica de orquestación del controller
    - Implementar caso de uso completo `RequestForecastAsync()`

16. ❌ Simplificar el Controller
    - Solo mapear HTTP request → Application Service
    - Solo mapear Application response → HTTP response

17. ❌ Implementar validación automática
    - Usar `FluentValidation` como filter automático
    - Eliminar validación manual en controllers

18. ❌ Separar concerns correctamente
    ```
    Controller (API Layer)
      ↓
    ForecastApplicationService (Application Layer)
      ↓
    Forecast Aggregate (Domain Layer)
      ↓
    IForecastRepository (Domain Interface)
      ↓
    ForecastRepository (Infrastructure Layer)
    ```

**Estimación**: 1-2 semanas

---

### Fase 5: Domain Events (MEDIA PRIORIDAD) 🟡

19. ❌ Definir interfaces base
    ```csharp
    public interface IDomainEvent
    {
        DateTimeOffset OccurredAt { get; }
    }
    ```

20. ❌ Implementar eventos de dominio
    - `ForecastRequestedEvent`
    - `ForecastDataUpdatedEvent`
    - `ForecastExpiredEvent`

21. ❌ Agregar colección de eventos al agregado
    ```csharp
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    ```

22. ❌ Implementar Event Publisher
    - Publicar eventos después de guardar el agregado
    - Limpiar eventos después de publicar

**Estimación**: 1 semana

---

### Fase 6: Refinamiento Avanzado (BAJA PRIORIDAD) 🟢

23. ⬜ Implementar Specifications (si necesario para queries complejas)
24. ⬜ Implementar Factories (si lógica de construcción se vuelve compleja)
25. ⬜ Documentar lenguaje ubicuo en un glosario
26. ⬜ Refactorizar hacia CQRS (separar Commands y Queries)

**Estimación**: Continuo

---

## 📊 Progreso General

| Fase | Estado | Progreso | Prioridad |
|------|--------|----------|-----------|
| Fase 1: Fundamentos | ⚠️ Parcial | 57% | ✅ En progreso |
| Fase 2: Validaciones | ❌ Pendiente | 0% | 🔴 ALTA |
| Fase 3: Comportamiento | ❌ Pendiente | 0% | 🔴 ALTA |
| Fase 4: Refactor App Layer | ❌ Pendiente | 0% | 🔴 ALTA |
| Fase 5: Domain Events | ❌ Pendiente | 0% | 🟡 MEDIA |
| Fase 6: Avanzado | ❌ Pendiente | 0% | 🟢 BAJA |

**Progreso Total del Proyecto DDD**: **57.8%** (puntuación 81/140)

---

## 🎯 Próximos Pasos Recomendados (Orden de Prioridad)

1. **🔴 URGENTE - Agregar validaciones a Value Objects** (Fase 2)
   - Previene estados inválidos
   - Protege integridad del dominio
   - Esfuerzo: Bajo, Impacto: Alto

2. **🔴 URGENTE - Refactorizar Application Service** (Fase 4)
   - Mueve lógica de orquestación del controller
   - Separa concerns correctamente
   - Esfuerzo: Medio, Impacto: Alto

3. **🔴 ALTA - Enriquecer modelo con comportamiento** (Fase 3)
   - Transforma modelo anémico en rico
   - Implementa lógica de negocio en el dominio
   - Esfuerzo: Medio, Impacto: Alto

4. **🟡 MEDIA - Implementar `ForecastId` y `WeatherCondition`** (Fase 1)
   - Mejora type safety
   - Elimina primitivos restantes
   - Esfuerzo: Bajo, Impacto: Medio

5. **🟡 MEDIA - Implementar Domain Events** (Fase 5)
   - Desacopla agregados
   - Facilita auditoría
   - Esfuerzo: Alto, Impacto: Medio

---

## 🎯 Conclusión Actualizada

**Fortalezas actuales** ✅:
- ✅ Arquitectura en capas bien definida
- ✅ Separación de concerns correcta
- ✅ Uso del patrón Repository
- ✅ Infraestructura desacoplada del dominio
- ✅ **Value Objects implementados** (`Temperature`, `Location`)
- ✅ **Agregado con identidad única** (`Guid Id`)
- ✅ **ProblemDetails RFC 7807 con OpenTelemetry**
- ✅ **API Versioning configurado**
- ✅ **Global Exception Handler con IExceptionHandler**

**Debilidades principales** ❌:
- ❌ **Value Objects sin validaciones** - permiten estados inválidos
- ❌ **Modelo de dominio anémico** - sin comportamiento ni lógica de negocio
- ❌ **Lógica de orquestación en el Controller** - debería estar en Application Service
- ❌ **Validaciones de negocio en Host Layer** - deberían estar en el dominio
- ❌ Sin eventos de dominio
- ❌ `Description` sigue como string primitivo (falta `WeatherCondition` VO)
- ❌ Uso de `{ get; init; }` en lugar de `{ get; private set; }`

**Progreso desde última revisión** 📈:
- Puntuación: **50/120 (41.6%)** → **81/140 (57.8%)**
- Mejora: **+16.2 puntos porcentuales**
- Fases completadas: Fase 1 al 57%

**Recomendación general**:
El proyecto ha realizado **avances significativos** hacia DDD con la implementación de Value Objects y mejor estructura del agregado. Sin embargo, para ser considerado verdadero DDD necesita:

1. **Agregar validaciones a los Value Objects** (crítico)
2. **Enriquecer el modelo con comportamiento** (crítico)
3. **Refactorizar Application Layer** para separar correctamente las responsabilidades (crítico)

Con estas tres mejoras críticas, el proyecto alcanzaría aproximadamente **85-90%** de adherencia a DDD táctico, lo cual es excelente para una aplicación de este tamaño.

**Estado actual**: **Arquitectura en capas con elementos de DDD** (Value Objects implementados pero incompletos)  
**Estado objetivo**: **DDD táctico completo** (modelo rico con comportamiento y validaciones)  
**Brecha**: **3 refactorizaciones críticas pendientes**

---

**Fecha de actualización**: 2025-12-27  
**Versión del análisis**: 2.0  
**Próxima revisión recomendada**: Después de completar Fase 2 (Validaciones) y Fase 4 (Refactor App Layer)

---

## 📚 Referencias Actualizadas

- **Libro**: "Domain-Driven Design" - Eric Evans (Blue Book)
- **Libro**: "Implementing Domain-Driven Design" - Vaughn Vernon (Red Book)
- **Libro**: "Domain-Driven Design Distilled" - Vaughn Vernon
- **Artículo**: [DDD Reference](https://www.domainlanguage.com/ddd/reference/) - Eric Evans
- **Código**: [eShopOnContainers](https://github.com/dotnet-architecture/eShopOnContainers) - Microsoft
- **RFC 7807**: [Problem Details for HTTP APIs](https://www.rfc-editor.org/rfc/rfc7807)
- **OpenTelemetry**: [.NET Observability](https://opentelemetry.io/docs/languages/net/)
- **ASP.NET Core**: [API Versioning](https://github.com/dotnet/aspnet-api-versioning)

---

## 📝 Apéndice: Comparativa de Implementaciones

### Comparativa: Antes vs Ahora vs Objetivo

#### Value Object `Temperature`

| Aspecto | Versión Anterior | Versión Actual | Objetivo DDD |
|---------|------------------|----------------|--------------|
| Tipo | `string` primitivo | Value Object | Value Object ✅ |
| Inmutabilidad | No aplica | ✅ Sí | ✅ Sí |
| Validaciones | ❌ No | ❌ No | ✅ Sí (unidades, rango) |
| Lógica dominio | ❌ No | ❌ No | ✅ Sí (conversiones, consultas) |
| Igualdad | Por referencia | ✅ Por valor | ✅ Por valor |
| Estado | 0% | 50% | 100% |

#### Agregado `Forecast`

| Aspecto | Versión Anterior | Versión Actual | Objetivo DDD |
|---------|------------------|----------------|--------------|
| Identidad | ❌ No | ✅ Guid | ✅ ForecastId VO |
| Value Objects | ❌ No | ⚠️ Parcial (2/3) | ✅ Completo (3/3) |
| Comportamiento | ❌ Anémico | ❌ Anémico | ✅ Rico |
| Invariantes | ❌ No | ❌ No | ✅ Protegidas |
| Setters | `{ get; init; }` | `{ get; init; }` | `{ get; private set; }` |
| Estado | 20% | 45% | 100% |

#### Application Layer

| Aspecto | Versión Anterior | Versión Actual | Objetivo DDD |
|---------|------------------|----------------|--------------|
| Ubicación lógica | Controller | ❌ Controller | ✅ Application Service |
| Orquestación | En controller | ❌ En controller | ✅ En service |
| Validación | Manual | ❌ Manual | ✅ Automática (filter) |
| Casos de uso | Difuso | ❌ Difuso | ✅ Explícito |
| Estado | 30% | 30% | 100% |

---

## 🔍 Análisis de Code Smells Actuales

### 1. Primitive Obsession (Parcialmente resuelto)
- ✅ `Location`: string → Value Object
- ✅ `Temperature`: string → Value Object
- ❌ `Description`: string → debería ser `WeatherCondition` VO
- ❌ `Id`: Guid → debería ser `ForecastId` VO

### 2. Anemic Domain Model (Pendiente)
- ❌ `Forecast` sin comportamiento
- ❌ `Temperature` sin lógica de conversión
- ❌ `Location` sin lógica de distancia

### 3. Feature Envy (Pendiente)
- ❌ Controller "envidiando" los datos de múltiples servicios
- ❌ Debería delegar a Application Service

### 4. Missing Validation (Crítico)
- ❌ Value Objects aceptan cualquier valor
- ❌ No hay protección contra estados inválidos

---

**FIN DEL ANÁLISIS DDD v2.0**

