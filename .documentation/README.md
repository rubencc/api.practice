# Weather API - Documentación Actualizada del Proyecto

**Fecha de actualización**: 2025-12-26

## 📋 Descripción General

Weather API es una aplicación .NET 8 que proporciona pronósticos meteorológicos basados en ubicación geográfica. El proyecto utiliza **Clean Architecture** con capas bien definidas y sigue los principios de **Domain-Driven Design (DDD)**.

### Características Principales
- 🌤️ Pronósticos meteorológicos en tiempo real
- 📍 Geolocalización mediante direcciones o coordenadas
- 💾 Almacenamiento persistente en MongoDB
- 🎯 API REST con Swagger/OpenAPI
- 🏗️ Clean Architecture y DDD
- 🔧 Patrón Options para configuración

---

## 🏗️ Arquitectura del Proyecto Actual

```
Api.Practice/
├── src/
│   ├── Weather.Domain/                  # Capa de Dominio
│   │   ├── Aggregates/
│   │   │   └── Forecast.cs              # Agregado raíz de pronóstico
│   │   ├── Repositories/
│   │   │   └── IForecastRepository.cs   # Interfaz de repositorio
│   │   ├── Exceptions/                  # (futuro)
│   │   ├── Entities/                    # (futuro)
│   │   ├── DomainServices/              # (futuro)
│   │   └── Validators/                  # (futuro)
│   │
│   ├── Weather.Application/             # Capa de Aplicación
│   │   ├── Commands/
│   │   │   └── ForecastCommand.cs       # Comando de pronóstico
│   │   ├── DTOs/
│   │   │   └── ForecastDto.cs           # DTO de respuesta
│   │   ├── Services/
│   │   │   ├── ForecastService.cs       # Servicio principal
│   │   │   ├── GeolocationService.cs    # Servicio OpenCage
│   │   │   └── WeatherQueryService.cs   # Servicio Open-Meteo
│   │   ├── Validators/
│   │   │   ├── AddressValidation.cs
│   │   │   └── DateTimeOffsetValidation.cs
│   │   ├── Interfaces/
│   │   │   ├── IGeolocationService.cs
│   │   │   ├── IValidation.cs
│   │   │   └── IWeatherQueryService.cs
│   │   ├── Configuration/
│   │   │   ├── HttpClientExtensions.cs
│   │   │   └── ServiceCollectionExtensions.cs
│   │   ├── Extensions/
│   │   │   └── DateTimeExtensions.cs
│   │   └── Queries/                     # (futuro)
│   │
│   ├── Weather.Infrastructure/          # Capa de Infraestructura
│   │   ├── Persistence/
│   │   │   ├── Repositories/
│   │   │   │   └── ForecastRepository.cs # Implementación MongoDB
│   │   │   └── Configurations/
│   │   │       ├── DefaultGuidMap.cs     # Mapeo GUID para MongoDB
│   │   │       └── ForecastMapping.cs    # Configuración de colección
│   │   ├── Configuration/
│   │   │   ├── MongoDbSettings.cs       # Settings (Options Pattern)
│   │   │   └── ServiceCollectionExtensions.cs
│   │   └── Messaging/                   # (vacío - futuro RabbitMQ)
│   │
│   └── Api.Weather.Host/                # Capa de Presentación
│       ├── Controllers/
│       │   └── WeatherController.cs     # API Controller
│       ├── Resources/
│       │   ├── ForecastRequest.cs       # Request DTO
│       │   └── ForecastResponse.cs      # Response DTO
│       ├── Properties/
│       │   └── launchSettings.json
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       └── Program.cs
│
├── tests/
│   └── Api.Weather.Host.UnitTest/       # Tests unitarios
│
├── .docker/                             # Docker configuration
│   ├── docker-compose.yaml              # MongoDB con replica set
│   └── .env                             # Variables de entorno
│
├── .documentation/                      # Documentación
│   ├── README.md                        # Documentación principal (este archivo)
│   ├── OPENCAGE_API_SETUP.md           # Setup OpenCage API
│   └── OPEN_METEO_SERVICE.md           # Documentación Open-Meteo
│
└── Api.Practice.sln                     # Archivo de solución
```

---

## 📦 Descripción de Capas

### 🎯 Weather.Domain (Dominio)

**Propósito**: Núcleo del negocio sin dependencias externas.

**Contenido actual**:
- **Aggregates/Forecast.cs**: Agregado raíz que representa un pronóstico meteorológico
- **Repositories/IForecastRepository.cs**: Interfaz del repositorio

**Características del Agregado Forecast**:
```csharp
public class Forecast
{
    public string Location { get; init; }      // Ubicación
    public DateTimeOffset Time { get; init; }  // Fecha/hora
    public string Temperature { get; init; }   // Temperatura
    public string Description { get; init; }   // Descripción clima
    
    // Factory method
    public static Forecast Create(string address, DateTimeOffset time, 
                                   string temperature, string description);
}
```

**Dependencias**: Ninguna (solo .NET 8)

---

### 📋 Weather.Application (Aplicación)

**Propósito**: Orquesta los casos de uso y coordina la lógica de aplicación.

**Servicios principales**:

1. **ForecastService**: Servicio principal que coordina la obtención y almacenamiento de pronósticos
2. **GeolocationService**: Integración con OpenCage API para convertir direcciones a coordenadas
3. **WeatherQueryService**: Integración con Open-Meteo API para obtener datos meteorológicos

**Validadores**:
- **AddressValidation**: Valida que la dirección no esté vacía
- **DateTimeOffsetValidation**: Valida rangos de fechas

**Configuración**:
- Registro de servicios HTTP con políticas de retry
- Inyección de dependencias

**Dependencias**:
- Weather.Domain (referencia de proyecto)
- Microsoft.Extensions.Http (8.0.0)
- Microsoft.Extensions.DependencyInjection.Abstractions (10.0.1)

---

### 🔧 Weather.Infrastructure (Infraestructura)

**Propósito**: Implementaciones técnicas de persistencia y servicios externos.

**Componentes principales**:

1. **ForecastRepository**: Implementación del repositorio usando MongoDB Driver con Options Pattern
2. **MongoDbSettings**: Configuración con **Options Pattern**
3. **Configuraciones de MongoDB**:
   - **ForecastMapping**: Configuración de la colección Forecast
   - **DefaultGuidMap**: Mapeo de GUID para MongoDB

**Patrón Options Implementado**:
```csharp
// Configuración en ServiceCollectionExtensions
services.Configure<MongoDbSettings>(
    configuration.GetSection("MongoDbSettings"));

services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
    return new MongoClient(settings.ConnectionString);
});

// Uso en ForecastRepository
public ForecastRepository(
    IMongoClient client, 
    IOptions<MongoDbSettings> settings)
{
    var mongoSettings = settings.Value;
    // ...
}
```

**Dependencias**:
- Weather.Domain (referencia de proyecto)
- Weather.Application (referencia de proyecto)
- MongoDB.Driver (3.5.2)
- Microsoft.Extensions.Configuration.Abstractions (10.0.1)
- Microsoft.Extensions.Configuration.Binder (10.0.1)
- Microsoft.Extensions.Options.ConfigurationExtensions (10.0.0)

---

### 🌐 Api.Weather.Host (Presentación)

**Propósito**: API REST HTTP que expone los endpoints.

**Controller principal**:
- **WeatherController**: Endpoint GET para obtener pronósticos

**Endpoint**:
```
GET /api/Weather
Content-Type: application/json

Request Body:
{
  "location": "Madrid, España",
  "time": "2025-12-26T10:00:00Z"
}

Response:
{
  "location": "Madrid, España",
  "time": "1735210800",
  "temperature": "15.2",
  "weather": "Parcialmente nublado"
}
```

**Características**:
- Swagger/OpenAPI para documentación interactiva
- Validaciones con validadores personalizados
- Manejo de errores

**Dependencias**:
- Weather.Application (referencia de proyecto)
- Weather.Infrastructure (referencia de proyecto)
- Swashbuckle.AspNetCore (6.5.0)

---

## 🔄 Flujo de Dependencias

```
Api.Weather.Host → Weather.Application → Weather.Domain
                          ↓
                  Weather.Infrastructure → Weather.Domain
```

**Regla fundamental**: Las dependencias siempre apuntan hacia el Domain

- ✅ Application puede usar Domain
- ✅ Infrastructure puede usar Domain y Application
- ✅ Host puede usar Application e Infrastructure
- ❌ Domain NO puede depender de otras capas
- ❌ Application NO puede depender de Infrastructure

---

## 🔧 Tecnologías y Servicios

### Base de Datos
- **MongoDB 8.0.12**: Base de datos NoSQL
- **MongoDB.Driver 3.5.2**: Driver oficial de .NET
- **Patrón Options**: Configuración fuertemente tipada

### APIs Externas Gratuitas

#### 1. OpenCage Geocoding API 🌍
- **Función**: Convertir direcciones a coordenadas (lat/lng)
- **Plan gratuito**: 2,500 peticiones/día
- **Requisitos**: API Key gratuita (sin tarjeta de crédito)
- **URL**: https://opencagedata.com/
- **Documentación**: [OPENCAGE_API_SETUP.md](OPENCAGE_API_SETUP.md)

#### 2. Open-Meteo Weather API ☀️
- **Función**: Datos meteorológicos en tiempo real
- **Plan gratuito**: Ilimitado, sin API Key
- **Características**: Basado en modelos meteorológicos globales
- **URL**: https://open-meteo.com
- **Documentación**: [OPEN_METEO_SERVICE.md](OPEN_METEO_SERVICE.md)

### Infraestructura
- **Docker**: Contenedores para MongoDB
- **Docker Compose**: Orquestación con replica set
- **.NET 8**: Framework principal

---

## 🚀 Guía de Configuración y Ejecución

### 1. Requisitos Previos
- ✅ .NET 8.0 SDK instalado
- ✅ Docker Desktop (para MongoDB)
- ✅ IDE: Visual Studio, Rider o VS Code

### 2. Configurar MongoDB

**Con Docker Compose**:
```bash
# Ir a la carpeta .docker
cd .docker

# Iniciar MongoDB con replica set
docker compose --profile infrastructure up -d

# Verificar que está corriendo
docker ps | grep mongodb

# Ver logs
docker logs mongodb

# Detener MongoDB
docker compose --profile infrastructure down
```

### 3. Configurar API Keys

Edita `src/Api.Weather.Host/appsettings.Development.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "OpenCage": {
    "ApiKey": "TU_API_KEY_AQUI"  // 👈 Reemplaza esto
  },
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "WeatherDb",
    "CollectionName": "Forecasts"
  }
}
```

**Obtener API Key de OpenCage** (2 minutos):
1. Ve a https://opencagedata.com/
2. Haz clic en "Sign Up"
3. Completa el registro (sin tarjeta de crédito)
4. Copia tu API Key del dashboard
5. Pégala en `appsettings.Development.json`

Ver guía detallada: [OPENCAGE_API_SETUP.md](OPENCAGE_API_SETUP.md)

### 4. Restaurar y Compilar
```bash
# Restaurar paquetes NuGet
dotnet restore

# Compilar toda la solución
dotnet build
```

### 5. Ejecutar la Aplicación
```bash
# Opción 1: Desde la raíz del proyecto
dotnet run --project src/Api.Weather.Host/Api.Weather.Host.csproj

# Opción 2: Desde la carpeta del proyecto
cd src/Api.Weather.Host
dotnet run
```

### 6. Acceder a la Aplicación

La API estará disponible en:
- 🌐 **HTTPS**: https://localhost:5001
- 🌐 **HTTP**: http://localhost:5000
- 📚 **Swagger UI**: https://localhost:5001/swagger

### 7. Probar la API

#### Con Swagger UI (Recomendado)
1. Abre https://localhost:5001/swagger
2. Haz clic en el endpoint `GET /api/Weather`
3. Haz clic en "Try it out"
4. Ingresa el body:
   ```json
   {
     "location": "Madrid, España",
     "time": "2025-12-26T10:00:00Z"
   }
   ```
5. Haz clic en "Execute"

#### Con curl
```bash
curl -X GET https://localhost:5001/api/Weather \
  -H "Content-Type: application/json" \
  -d '{
    "location": "Madrid, España",
    "time": "2025-12-26T10:00:00Z"
  }' \
  -k
```

#### Respuesta esperada
```json
{
  "location": "Madrid, España",
  "time": "1735210800",
  "temperature": "15.2",
  "weather": "Parcialmente nublado"
}
```

---

## 🗄️ MongoDB - Persistencia de Datos

### Configuración con Options Pattern

El proyecto utiliza el **Options Pattern** de .NET para una configuración fuertemente tipada y testeable:

```csharp
// 1. Clase de configuración
public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string CollectionName { get; set; } = "Forecasts";
}

// 2. Registro en DI
services.Configure<MongoDbSettings>(
    configuration.GetSection("MongoDbSettings"));

// 3. Inyección en servicios
public ForecastRepository(
    IMongoClient client, 
    IOptions<MongoDbSettings> settings)
{
    var config = settings.Value;
    // Usar config.ConnectionString, config.DatabaseName, etc.
}
```

**Beneficios del Options Pattern**:
- ✅ Configuración fuertemente tipada
- ✅ Validación en tiempo de compilación
- ✅ Fácil de testear con mocks
- ✅ Soporte para recarga en caliente (con IOptionsSnapshot)
- ✅ Integración perfecta con DI

### Estructura de la Base de Datos

**Base de datos**: `WeatherDb`
**Colección**: `Forecasts`

**Documento de ejemplo**:
```json
{
  "_id": ObjectId("507f1f77bcf86cd799439011"),
  "location": "Madrid, España",
  "time": ISODate("2025-12-26T10:00:00Z"),
  "temperature": "15.2",
  "description": "Parcialmente nublado"
}
```

### Comandos Útiles de MongoDB

```bash
# Conectarse al shell de MongoDB
docker exec -it mongodb mongosh

# Dentro del shell:
show dbs                          # Ver bases de datos
use WeatherDb                     # Usar la BD del proyecto
show collections                  # Ver colecciones
db.Forecasts.find().pretty()      # Ver todos los pronósticos
db.Forecasts.countDocuments()     # Contar documentos

# Buscar por ubicación
db.Forecasts.find({ location: "Madrid, España" }).pretty()

# Eliminar todos los documentos
db.Forecasts.deleteMany({})
```

### Gestión de MongoDB

```bash
# Iniciar MongoDB
cd .docker
docker compose --profile infrastructure up -d

# Detener MongoDB
docker compose --profile infrastructure down

# Reiniciar MongoDB
docker compose --profile infrastructure restart

# Ver logs en tiempo real
docker logs -f mongodb

# Ver estado del contenedor
docker ps -f name=mongodb

# Abrir MongoDB Shell interactivo
docker exec -it mongodb mongosh

# Limpiar datos (eliminar contenedor y volúmenes)
docker compose --profile infrastructure down -v
```

Ver documentación de Docker Compose: `.docker/docker-compose.yaml`

---

## 📐 Patrones y Principios Aplicados

### Patrones de Arquitectura
- ✅ **Clean Architecture**: Separación clara de responsabilidades por capas
- ✅ **Domain-Driven Design (DDD)**: El dominio como centro de la arquitectura
- ✅ **Dependency Inversion Principle**: Dependencias apuntan hacia abstracciones
- ✅ **Separation of Concerns**: Cada capa tiene una responsabilidad específica

### Patrones de Diseño
- ✅ **Repository Pattern**: Abstracción del acceso a datos
- ✅ **Factory Pattern**: Método Create en Forecast
- ✅ **Options Pattern**: Configuración fuertemente tipada
- ✅ **Dependency Injection**: Inversión de control con .NET DI
- ✅ **Service Layer Pattern**: Servicios de aplicación

### Patrones de Persistencia
- ✅ **Document Store Pattern**: MongoDB para almacenamiento NoSQL
- ✅ **Singleton Pattern**: MongoClient compartido (recomendación oficial de MongoDB)

### Principios SOLID
- ✅ **Single Responsibility**: Cada clase tiene una responsabilidad
- ✅ **Open/Closed**: Abierto para extensión, cerrado para modificación
- ✅ **Liskov Substitution**: Las abstracciones son sustituibles
- ✅ **Interface Segregation**: Interfaces específicas (IGeolocationService, IWeatherQueryService)
- ✅ **Dependency Inversion**: Dependencia de abstracciones, no de implementaciones

---

## 🔮 Extensiones Futuras Planificadas

### Corto Plazo
- [ ] Tests unitarios completos
- [ ] Tests de integración con Testcontainers
- [ ] Logging estructurado con Serilog
- [ ] Health checks para MongoDB y APIs externas
- [ ] Cache en memoria para pronósticos recientes

### Medio Plazo
- [ ] Implementación de CQRS con handlers separados
- [ ] Value Objects (PostalCode, Temperature, Coordinates)
- [ ] Domain Events
- [ ] Paginación de resultados
- [ ] Búsqueda histórica de pronósticos

### Largo Plazo
- [ ] Integración con RabbitMQ y MassTransit para mensajería asíncrona
- [ ] Implementación de Outbox Pattern para consistencia eventual
- [ ] Cache distribuido con Redis
- [ ] Implementación de Circuit Breaker con Polly
- [ ] API de GraphQL además de REST
- [ ] Autenticación y autorización con JWT
- [ ] Rate limiting
- [ ] Versionado de API

---

## 📚 Documentación Adicional

- [OPENCAGE_API_SETUP.md](OPENCAGE_API_SETUP.md) - Configuración de OpenCage API
- [OPEN_METEO_SERVICE.md](OPEN_METEO_SERVICE.md) - Documentación de Open-Meteo
- `.docker/docker-compose.yaml` - Configuración de MongoDB con Docker Compose

---

## 🛠️ Solución de Problemas Comunes

### Error: "Unable to connect to MongoDB"
```bash
# Verificar que MongoDB está corriendo
docker ps | grep mongodb

# Ver logs de MongoDB
docker logs mongodb

# Reiniciar MongoDB
cd .docker
docker compose --profile infrastructure restart
```

### Error: "MongoServerError: No host described in new configuration"
Este error ocurre cuando el replica set está mal configurado. Solución:
```bash
# Detener y eliminar contenedor
docker stop mongodb && docker rm mongodb

# Eliminar volumen
docker volume rm docker_mongodb_data

# Iniciar nuevamente
cd .docker
docker compose --profile infrastructure up -d
```

### Error: "OpenCage API Key is missing or invalid"
- Verifica que la API Key esté en `appsettings.Development.json`
- Asegúrate de que la key sea válida en https://opencagedata.com/dashboard
- Revisa que no haya espacios extra o caracteres invisibles

### Error de compilación
```bash
# Limpiar y recompilar
dotnet clean
dotnet restore
dotnet build
```

### Puerto 27017 ya en uso
```bash
# Ver qué proceso usa el puerto
sudo lsof -i :27017

# O detener MongoDB local si está corriendo
sudo systemctl stop mongod
```

---

## 👥 Contribuir

Para contribuir al proyecto:
1. Hacer fork del repositorio
2. Crear una rama feature (`git checkout -b feature/NuevaFuncionalidad`)
3. Hacer commit de los cambios (`git commit -m 'Agregar nueva funcionalidad'`)
4. Push a la rama (`git push origin feature/NuevaFuncionalidad`)
5. Crear un Pull Request

---

## 📝 Notas de Implementación

### ¿Por qué NO se usa MediatR?
Este proyecto implementa CQRS sin MediatR para mantener la simplicidad. Los servicios se inyectan directamente vía DI. Esto proporciona:
- Mayor claridad en el flujo de código
- Menos abstracciones y complejidad
- Stack traces más simples para debugging
- Menor curva de aprendizaje

**Considerar MediatR** cuando el proyecto crezca y necesite:
- Pipeline behaviors automáticos
- Múltiples handlers por mensaje
- Desacoplamiento extremo

### ¿Por qué MongoDB y no SQL?
- Esquema flexible para diferentes tipos de pronósticos
- Excelente rendimiento en operaciones de lectura
- Mapeo natural con objetos C# (documentos JSON)
- Escalabilidad horizontal con sharding

### ¿Por qué Open-Meteo?
- Totalmente gratuito sin límites
- No requiere API Key ni registro
- Datos precisos basados en modelos meteorológicos profesionales
- Alta disponibilidad (99.9% uptime)

---

**Última actualización**: 2025-12-26
**Versión**: 1.0.0
**Framework**: .NET 8
**Licencia**: MIT

