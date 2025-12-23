# Estructura de Proyecto DDD (Domain-Driven Design)

## Organización por Capas

La arquitectura DDD organiza el código en capas bien definidas, cada una con responsabilidades específicas y dependencias controladas.

```
Solution/
├── src/
│   ├── Domain/                          # Núcleo del negocio
│   │   ├── Entities/                    # Entidades del dominio
│   │   ├── ValueObjects/                # Objetos de valor
│   │   ├── Aggregates/                  # Agregados (raíces)
│   │   ├── DomainServices/              # Servicios de dominio
│   │   ├── DomainEvents/                # Eventos de dominio
│   │   ├── Repositories/                # Interfaces de repositorios
│   │   └── Exceptions/                  # Excepciones de negocio
│   │
│   ├── Application/                     # Casos de uso
│   │   ├── Commands/                    # Comandos (escritura)
│   │   ├── Queries/                     # Consultas (lectura)
│   │   ├── DTOs/                        # Data Transfer Objects
│   │   ├── Validators/                  # Validaciones de aplicación
│   │   └── Interfaces/                  # Contratos de servicios externos
│   │
│   ├── Infrastructure/                  # Implementaciones técnicas
│   │   ├── Persistence/                 # Base de datos MongoDB
│   │   │   ├── Repositories/            # Implementación de repositorios
│   │   │   ├── Configurations/          # Mapeo MongoDB
│   │   │   └── Context/                 # Contexto MongoDB
│   │   ├── ExternalServices/            # APIs externas (ej: AEMET, OpenWeatherMap)
│   │   │   ├── Weather/                 # Servicios meteorológicos
│   │   │   └── Http/                    # Clients HTTP
│   │   ├── Messaging/                   # Bus de eventos y mensajería
│   │   │   ├── Consumers/               # Consumidores MassTransit
│   │   │   ├── Publishers/              # Publicadores de eventos
│   │   │   └── Configuration/           # Configuración RabbitMQ
│   │   └── Caching/                     # Caché
│   │
│   └── Api/Presentation/                # Capa de presentación
│       ├── Controllers/                 # Controladores HTTP
│       ├── Filters/                     # Filtros y middleware
│       ├── Resources/                   # Modelos de request/response
│       └── Mappings/                    # AutoMapper profiles
│
└── tests/
    ├── Domain.Tests/
    ├── Application.Tests/
    └── Api.Tests/
```

---

## Descripción de Capas y Proyectos

### 🎯 **Domain (Dominio)**

**Propósito**: Contiene la lógica de negocio pura, sin dependencias externas.

**Carpetas**:

- **Entities/**: Entidades con identidad única (ej: `Forecast`, `User`)
  - Contienen lógica de negocio
  - Tienen un identificador único
  - Representan conceptos del negocio

- **ValueObjects/**: Objetos inmutables sin identidad (ej: `PostalCode`, `Temperature`)
  - Se comparan por valor, no por identidad
  - Son inmutables
  - Encapsulan validaciones

- **Aggregates/**: Agrupaciones de entidades que se tratan como una unidad
  - Define límites transaccionales
  - La raíz del agregado controla el acceso

- **DomainServices/**: Servicios de dominio para lógica que no pertenece a una entidad
  - Operaciones que involucran múltiples entidades
  - Lógica de negocio compleja

- **DomainEvents/**: Eventos que representan algo que ocurrió en el dominio
  - Comunicación entre agregados
  - Desacoplamiento

- **Repositories/**: Interfaces (contratos) para acceso a datos
  - Define qué operaciones se pueden hacer
  - La implementación está en Infrastructure

- **Exceptions/**: Excepciones específicas del dominio
  - Errores de negocio
  - Validaciones de dominio

**Dependencias**: Ninguna (solo .NET base)

---

### 📋 **Application (Aplicación)**

**Propósito**: Orquesta los casos de uso del sistema. Coordina el flujo de datos entre Domain e Infrastructure.

**Carpetas**:

- **Commands/**: Operaciones de escritura (CQRS)
  - Modifican el estado del sistema
  - Implementados como servicios directos (sin MediatR)
  - Ejemplo: `CreateForecastCommand`, `RequestForecastCommand`

- **Queries/**: Operaciones de lectura (CQRS)
  - Solo consultan datos
  - Implementados como servicios directos (sin MediatR)
  - Ejemplo: `GetForecastQuery`, `GetHistoricalForecastQuery`

- **Services/**: Servicios de aplicación
  - Coordinan la lógica de negocio
  - Ejemplo: `ForecastApplicationService`

- **DTOs/**: Data Transfer Objects
  - Objetos para transferir datos entre capas
  - No contienen lógica de negocio

- **Validators/**: Validadores de entrada (FluentValidation)
  - Validaciones de aplicación
  - Complementan las validaciones de dominio

- **Interfaces/**: Contratos de servicios externos
  - Define cómo la aplicación usa servicios externos
  - Implementaciones en Infrastructure

**Dependencias**: Domain

**Nota**: Este proyecto usa CQRS sin MediatR. Los Commands y Queries son servicios que se inyectan directamente vía DI.

---

### 🔧 **Infrastructure (Infraestructura)**

**Propósito**: Implementa los detalles técnicos y dependencias externas.

**Carpetas**:

- **Persistence/**: Acceso a base de datos MongoDB
  - **Repositories/**: Implementaciones de `IRepository`
  - **Configurations/**: Configuraciones de colecciones y índices MongoDB
  - **Context/**: Contexto de MongoDB con IMongoDatabase

- **ExternalServices/**: Integraciones con APIs externas
  - **Weather/**: Implementaciones de servicios meteorológicos
    - `AemetWeatherService.cs`: Cliente AEMET
    - `OpenWeatherMapService.cs`: Cliente OpenWeatherMap
  - **Http/**: HttpClient factories y configuraciones
    - Políticas de retry (Polly)
    - Circuit breakers

- **Messaging/**: Sistema de mensajería con RabbitMQ y MassTransit
  - **Consumers/**: Consumidores de mensajes
    - `ForecastRequestedConsumer.cs`: Procesa solicitudes de pronóstico
    - `WeatherDataUpdatedConsumer.cs`: Procesa actualizaciones
  - **Publishers/**: Publicadores de eventos
    - `EventPublisher.cs`: Publica eventos de dominio
  - **Configuration/**: Configuración de RabbitMQ y MassTransit
    - `RabbitMqConfiguration.cs`: Settings de RabbitMQ
    - `MassTransitConfiguration.cs`: Registro de MassTransit

- **Caching/**: Implementación de caché
  - Redis, Memory Cache
  - Estrategias de caché

**Dependencias**: Domain, Application

**Tecnologías**:
- **MongoDB.Driver**: Base de datos NoSQL
- **MassTransit**: Abstracción de mensajería
- **RabbitMQ.Client**: Message broker
- **Polly**: Resiliencia y reintentos
- **Redis** (opcional): Cache distribuido

---

### 🌐 **Api/Presentation (Presentación)**

**Propósito**: Expone la API HTTP y maneja las peticiones web.

**Carpetas**:

- **Controllers/**: Endpoints HTTP
  - Reciben peticiones HTTP
  - Delegan a Application layer
  - Retornan respuestas HTTP

- **Filters/**: Filtros y middleware
  - Validación global
  - Manejo de errores
  - Logging

- **Resources/**: Modelos de request/response
  - Contratos de la API
  - Validaciones de entrada

- **Mappings/**: Perfiles de AutoMapper
  - Mapeo entre DTOs y Resources
  - Transformación de datos

**Dependencias**: Application, Infrastructure

---

## Flujo de Dependencias

```
Api/Presentation → Application → Domain
                        ↓
                 Infrastructure → Domain
```

**Regla de oro**: Las dependencias siempre apuntan hacia el Domain (centro)

- ✅ Application puede usar Domain
- ✅ Infrastructure puede usar Domain
- ✅ Api puede usar Application
- ❌ Domain NO puede depender de ninguna otra capa

---

## Aplicación al Proyecto Weather

### Estructura Recomendada

```
Api.Weather/
├── Api.Weather.Domain/
│   ├── Entities/
│   │   ├── Forecast.cs                  # Entidad de pronóstico (agregado raíz)
│   │   └── WeatherAlert.cs              # Alertas meteorológicas
│   ├── ValueObjects/
│   │   ├── PostalCode.cs                # Código postal validado
│   │   ├── Temperature.cs               # Temperatura con unidad
│   │   ├── WeatherDate.cs               # Fecha del pronóstico
│   │   └── Coordinates.cs               # Latitud y longitud
│   ├── DomainEvents/
│   │   ├── ForecastRequestedEvent.cs    # Evento: pronóstico solicitado
│   │   └── ForecastCreatedEvent.cs      # Evento: pronóstico creado
│   ├── Repositories/
│   │   ├── IForecastRepository.cs       # Contrato de repositorio
│   │   └── IWeatherCacheRepository.cs   # Contrato de caché
│   └── Exceptions/
│       ├── InvalidPostalCodeException.cs
│       ├── ForecastNotFoundException.cs
│       └── WeatherServiceUnavailableException.cs
│
├── Api.Weather.Application/
│   ├── Services/
│   │   ├── ForecastApplicationService.cs    # Servicio principal de pronósticos
│   │   └── WeatherQueryService.cs           # Servicio de consultas
│   ├── Commands/
│   │   └── RequestForecastCommand.cs        # DTO/Modelo de comando
│   ├── Queries/
│   │   ├── GetForecastQuery.cs              # DTO/Modelo de query
│   │   └── GetHistoricalForecastQuery.cs    # DTO/Modelo de query histórico
│   ├── DTOs/
│   │   ├── ForecastDto.cs                   # DTO de respuesta
│   │   ├── ForecastRequestDto.cs            # DTO de petición
│   │   └── WeatherDataDto.cs                # DTO de datos meteorológicos
│   ├── Validators/
│   │   ├── PostalCodeValidator.cs           # Validador de código postal
│   │   └── DateValidator.cs                 # Validador de fecha
│   ├── Interfaces/
│   │   ├── IWeatherService.cs               # Contrato de servicio externo
│   │   ├── IEventPublisher.cs               # Contrato de publicador de eventos
│   │   ├── IForecastApplicationService.cs   # Contrato de servicio de aplicación
│   │   └── IWeatherQueryService.cs          # Contrato de servicio de consultas
│   └── Mappers/
│       └── ForecastMapper.cs            # Mapeo entre entidades y DTOs
│
├── Api.Weather.Infrastructure/
│   ├── Persistence/
│   │   ├── Context/
│   │   │   ├── MongoDbContext.cs        # Contexto MongoDB
│   │   │   └── MongoDbSettings.cs       # Configuración MongoDB
│   │   ├── Repositories/
│   │   │   ├── ForecastRepository.cs    # Implementación repositorio
│   │   │   └── WeatherCacheRepository.cs
│   │   └── Configurations/
│   │       ├── ForecastConfiguration.cs # Configuración colección Forecast
│   │       └── IndexConfiguration.cs    # Índices MongoDB
│   │
│   ├── ExternalServices/
│   │   ├── Weather/
│   │   │   ├── AemetWeatherService.cs   # Implementación AEMET
│   │   │   ├── OpenWeatherMapService.cs # Implementación OpenWeatherMap
│   │   │   └── WeatherServiceFactory.cs # Factory para servicios
│   │   └── Http/
│   │       ├── HttpClientConfiguration.cs
│   │       └── PollyPolicies.cs         # Políticas de resiliencia
│   │
│   ├── Messaging/
│   │   ├── Consumers/
│   │   │   ├── ForecastRequestedConsumer.cs    # Consume solicitudes
│   │   │   └── WeatherDataUpdatedConsumer.cs   # Consume actualizaciones
│   │   ├── Publishers/
│   │   │   └── EventPublisher.cs               # Publica eventos de dominio
│   │   ├── Configuration/
│   │   │   ├── RabbitMqSettings.cs             # Settings RabbitMQ
│   │   │   └── MassTransitConfiguration.cs     # Configuración MassTransit
│   │   └── Messages/
│   │       ├── ForecastRequestedMessage.cs     # Mensaje de solicitud
│   │       └── ForecastCreatedMessage.cs       # Mensaje de creación
│   │
│   ├── Caching/
│   │   ├── RedisCacheService.cs         # Servicio de caché Redis
│   │   └── CacheSettings.cs             # Configuración de caché
│   │
│   └── DependencyInjection.cs           # Registro de servicios Infrastructure
│
├── Api.Weather.Host/
│   ├── Controllers/
│   │   ├── WeatherController.cs         # Endpoint HTTP
│   │   └── HealthController.cs          # Health checks
│   ├── Resources/
│   │   ├── ForecastRequest.cs           # Modelo de entrada
│   │   └── ForecastResponse.cs          # Modelo de salida
│   ├── Extensions/
│   │   └── ServiceCollectionExtensions.cs # DI configuration
│   ├── Middlewares/
│   │   ├── ExceptionHandlerMiddleware.cs # Manejo de errores
│   │   └── LoggingMiddleware.cs          # Logging de requests
│   ├── appsettings.json                  # Configuración general
│   ├── appsettings.Development.json      # Configuración desarrollo
│   └── Program.cs                        # Entry point
│
└── tests/
    ├── Api.Weather.Domain.Tests/
    │   ├── Entities/
    │   └── ValueObjects/
    ├── Api.Weather.Application.Tests/
    │   ├── Commands/
    │   └── Queries/
    ├── Api.Weather.Infrastructure.Tests/
    │   ├── Repositories/
    │   └── ExternalServices/
    └── Api.Weather.Host.Tests/
        └── Controllers/
```

---

## Dependencias Entre Proyectos

### Diagrama de Dependencias

```
┌─────────────────────────────────────────────────────────────┐
│                                                             │
│                    Api.Weather.Host                         │
│                   (Presentation Layer)                      │
│                                                             │
└──────────────┬──────────────────────────┬───────────────────┘
               │                          │
               │ referencia               │ referencia
               ▼                          ▼
┌──────────────────────────┐   ┌──────────────────────────────┐
│                          │   │                              │
│  Api.Weather.Application │   │  Api.Weather.Infrastructure  │
│   (Application Layer)    │   │   (Infrastructure Layer)     │
│                          │   │                              │
└────────────┬─────────────┘   └──────────┬───────────────────┘
             │                            │
             │ referencia                 │ referencia
             ▼                            ▼
┌────────────────────────────────────────────────────────────┐
│                                                            │
│                  Api.Weather.Domain                        │
│                   (Domain Layer)                           │
│              ¡Sin dependencias externas!                   │
│                                                            │
└────────────────────────────────────────────────────────────┘

Tests:
┌─────────────────────┐     ┌─────────────────────┐     ┌─────────────────────┐
│  Domain.Tests       │────▶│  Application.Tests  │────▶│  Infrastructure     │
│  (solo Domain)      │     │  (Domain + App)     │     │  .Tests             │
└─────────────────────┘     └─────────────────────┘     │  (Domain+App+Infra) │
                                                         └─────────────────────┘
                                    ┌─────────────────────────────┐
                                    │  Host.Tests                 │
                                    │  (todos los proyectos)      │
                                    └─────────────────────────────┘
```

---

### Configuración de Referencias en .csproj

#### 1️⃣ **Api.Weather.Domain**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <!-- Sin referencias a otros proyectos -->
  <!-- Solo depende de .NET base -->
</Project>
```

**Dependencias**: Ninguna

---

#### 2️⃣ **Api.Weather.Application**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <!-- Referencias a otros proyectos -->
  <ItemGroup>
    <ProjectReference Include="..\Api.Weather.Domain\Api.Weather.Domain.csproj" />
  </ItemGroup>

  <!-- Paquetes NuGet -->
  <ItemGroup>
    <PackageReference Include="FluentValidation" Version="11.9.0" />
  </ItemGroup>
</Project>
```

**Dependencias**:
- ✅ `Api.Weather.Domain` (referencia de proyecto)

---

#### 3️⃣ **Api.Weather.Infrastructure**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <!-- Referencias a otros proyectos -->
  <ItemGroup>
    <ProjectReference Include="..\Api.Weather.Domain\Api.Weather.Domain.csproj" />
    <ProjectReference Include="..\Api.Weather.Application\Api.Weather.Application.csproj" />
  </ItemGroup>

  <!-- Paquetes NuGet -->
  <ItemGroup>
    <!-- MongoDB -->
    <PackageReference Include="MongoDB.Driver" Version="2.23.1" />
    
    <!-- MassTransit y RabbitMQ -->
    <PackageReference Include="MassTransit" Version="8.1.3" />
    <PackageReference Include="MassTransit.RabbitMQ" Version="8.1.3" />
    
    <!-- HttpClient y Resiliencia -->
    <PackageReference Include="Microsoft.Extensions.Http" Version="8.0.0" />
    <PackageReference Include="Microsoft.Extensions.Http.Polly" Version="8.0.0" />
    <PackageReference Include="Polly" Version="8.2.0" />
    
    <!-- Caché (opcional) -->
    <PackageReference Include="StackExchange.Redis" Version="2.7.10" />
  </ItemGroup>
</Project>
```

**Dependencias**:
- ✅ `Api.Weather.Domain` (referencia de proyecto)
- ✅ `Api.Weather.Application` (referencia de proyecto)

---

#### 4️⃣ **Api.Weather.Host**

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <!-- Referencias a otros proyectos -->
  <ItemGroup>
    <ProjectReference Include="..\Api.Weather.Application\Api.Weather.Application.csproj" />
    <ProjectReference Include="..\Api.Weather.Infrastructure\Api.Weather.Infrastructure.csproj" />
  </ItemGroup>

  <!-- Paquetes NuGet -->
  <ItemGroup>
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
    <PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
    <PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
  </ItemGroup>
</Project>
```

**Dependencias**:
- ✅ `Api.Weather.Application` (referencia de proyecto)
- ✅ `Api.Weather.Infrastructure` (referencia de proyecto)
- ⚠️ **NO** referencia directamente a `Domain` (lo obtiene transitivamente)

---

### Proyectos de Tests

#### 5️⃣ **Api.Weather.Domain.Tests**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <!-- Referencias a otros proyectos -->
  <ItemGroup>
    <ProjectReference Include="..\..\src\Api.Weather.Domain\Api.Weather.Domain.csproj" />
  </ItemGroup>

  <!-- Paquetes de Testing -->
  <ItemGroup>
    <PackageReference Include="xUnit" Version="2.6.4" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.6" />
    <PackageReference Include="NSubstitute" Version="5.1.0" />
    <PackageReference Include="AwesomeAssertions" Version="9.3.0" />
  </ItemGroup>
</Project>
```

**Dependencias**:
- ✅ `Api.Weather.Domain` (referencia de proyecto)

---

#### 6️⃣ **Api.Weather.Application.Tests**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <!-- Referencias a otros proyectos -->
  <ItemGroup>
    <ProjectReference Include="..\..\src\Api.Weather.Domain\Api.Weather.Domain.csproj" />
    <ProjectReference Include="..\..\src\Api.Weather.Application\Api.Weather.Application.csproj" />
  </ItemGroup>

  <!-- Paquetes de Testing -->
  <ItemGroup>
    <PackageReference Include="xUnit" Version="2.6.4" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.6" />
    <PackageReference Include="NSubstitute" Version="5.1.0" />
    <PackageReference Include="AwesomeAssertions" Version="9.3.0" />
  </ItemGroup>
</Project>
```

**Dependencias**:
- ✅ `Api.Weather.Domain` (referencia de proyecto)
- ✅ `Api.Weather.Application` (referencia de proyecto)

---

#### 7️⃣ **Api.Weather.Infrastructure.Tests**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <!-- Referencias a otros proyectos -->
  <ItemGroup>
    <ProjectReference Include="..\..\src\Api.Weather.Domain\Api.Weather.Domain.csproj" />
    <ProjectReference Include="..\..\src\Api.Weather.Application\Api.Weather.Application.csproj" />
    <ProjectReference Include="..\..\src\Api.Weather.Infrastructure\Api.Weather.Infrastructure.csproj" />
  </ItemGroup>

  <!-- Paquetes de Testing -->
  <ItemGroup>
    <PackageReference Include="xUnit" Version="2.6.4" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.6" />
    <PackageReference Include="NSubstitute" Version="5.1.0" />
    <PackageReference Include="AwesomeAssertions" Version="9.3.0" />
    <PackageReference Include="Testcontainers.MongoDb" Version="3.6.0" />
    <PackageReference Include="Testcontainers.RabbitMq" Version="3.6.0" />
  </ItemGroup>
</Project>
```

**Dependencias**:
- ✅ `Api.Weather.Domain` (referencia de proyecto)
- ✅ `Api.Weather.Application` (referencia de proyecto)
- ✅ `Api.Weather.Infrastructure` (referencia de proyecto)

---

#### 8️⃣ **Api.Weather.Host.Tests**

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <!-- Referencias a otros proyectos -->
  <ItemGroup>
    <ProjectReference Include="..\..\src\Api.Weather.Domain\Api.Weather.Domain.csproj" />
    <ProjectReference Include="..\..\src\Api.Weather.Application\Api.Weather.Application.csproj" />
    <ProjectReference Include="..\..\src\Api.Weather.Infrastructure\Api.Weather.Infrastructure.csproj" />
    <ProjectReference Include="..\..\src\Api.Weather.Host\Api.Weather.Host.csproj" />
  </ItemGroup>

  <!-- Paquetes de Testing -->
  <ItemGroup>
    <PackageReference Include="xUnit" Version="2.6.4" />
    <PackageReference Include="xunit.runner.visualstudio" Version="2.5.6" />
    <PackageReference Include="NSubstitute" Version="5.1.0" />
    <PackageReference Include="AwesomeAssertions" Version="9.3.0" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
    <PackageReference Include="Testcontainers.MongoDb" Version="3.6.0" />
    <PackageReference Include="Testcontainers.RabbitMq" Version="3.6.0" />
  </ItemGroup>
</Project>
```

**Dependencias**:
- ✅ `Api.Weather.Domain` (referencia de proyecto)
- ✅ `Api.Weather.Application` (referencia de proyecto)
- ✅ `Api.Weather.Infrastructure` (referencia de proyecto)
- ✅ `Api.Weather.Host` (referencia de proyecto)

---

### Resumen de Dependencias

| Proyecto | Depende de | Tipo de Proyecto |
|----------|-----------|------------------|
| **Domain** | Ninguno | Class Library |
| **Application** | Domain | Class Library |
| **Infrastructure** | Domain + Application | Class Library |
| **Host** | Application + Infrastructure | Web Application |
| **Domain.Tests** | Domain | Test Project |
| **Application.Tests** | Domain + Application | Test Project |
| **Infrastructure.Tests** | Domain + Application + Infrastructure | Test Project |
| **Host.Tests** | Domain + Application + Infrastructure + Host | Test Project |

---

### Reglas de Dependencias

#### ✅ **Permitido**

1. **Application** puede referenciar **Domain**
2. **Infrastructure** puede referenciar **Domain** y **Application**
3. **Host** puede referenciar **Application** e **Infrastructure**
4. Los proyectos de test pueden referenciar los proyectos que prueban

#### ❌ **Prohibido**

1. **Domain** NO puede referenciar ningún otro proyecto
2. **Application** NO puede referenciar **Infrastructure** o **Host**
3. **Infrastructure** NO puede referenciar **Host**
4. Referencias circulares entre proyectos

---

### Comandos para Agregar Referencias

Si necesitas crear los proyectos y agregar las referencias, usa estos comandos:

```bash
# Crear solución
dotnet new sln -n Api.Practice

# Crear proyectos
dotnet new classlib -n Api.Weather.Domain -o src/Api.Weather.Domain
dotnet new classlib -n Api.Weather.Application -o src/Api.Weather.Application
dotnet new classlib -n Api.Weather.Infrastructure -o src/Api.Weather.Infrastructure
dotnet new webapi -n Api.Weather.Host -o src/Api.Weather.Host

# Crear proyectos de tests
dotnet new xunit -n Api.Weather.Domain.Tests -o tests/Api.Weather.Domain.Tests
dotnet new xunit -n Api.Weather.Application.Tests -o tests/Api.Weather.Application.Tests
dotnet new xunit -n Api.Weather.Infrastructure.Tests -o tests/Api.Weather.Infrastructure.Tests
dotnet new xunit -n Api.Weather.Host.Tests -o tests/Api.Weather.Host.Tests

# Agregar proyectos a la solución
dotnet sln add src/Api.Weather.Domain/Api.Weather.Domain.csproj
dotnet sln add src/Api.Weather.Application/Api.Weather.Application.csproj
dotnet sln add src/Api.Weather.Infrastructure/Api.Weather.Infrastructure.csproj
dotnet sln add src/Api.Weather.Host/Api.Weather.Host.csproj
dotnet sln add tests/Api.Weather.Domain.Tests/Api.Weather.Domain.Tests.csproj
dotnet sln add tests/Api.Weather.Application.Tests/Api.Weather.Application.Tests.csproj
dotnet sln add tests/Api.Weather.Infrastructure.Tests/Api.Weather.Infrastructure.Tests.csproj
dotnet sln add tests/Api.Weather.Host.Tests/Api.Weather.Host.Tests.csproj

# Agregar referencias entre proyectos de src/
cd src/Api.Weather.Application
dotnet add reference ../Api.Weather.Domain/Api.Weather.Domain.csproj

cd ../Api.Weather.Infrastructure
dotnet add reference ../Api.Weather.Domain/Api.Weather.Domain.csproj
dotnet add reference ../Api.Weather.Application/Api.Weather.Application.csproj

cd ../Api.Weather.Host
dotnet add reference ../Api.Weather.Application/Api.Weather.Application.csproj
dotnet add reference ../Api.Weather.Infrastructure/Api.Weather.Infrastructure.csproj

# Agregar referencias en proyectos de tests/
cd ../../tests/Api.Weather.Domain.Tests
dotnet add reference ../../src/Api.Weather.Domain/Api.Weather.Domain.csproj

cd ../Api.Weather.Application.Tests
dotnet add reference ../../src/Api.Weather.Domain/Api.Weather.Domain.csproj
dotnet add reference ../../src/Api.Weather.Application/Api.Weather.Application.csproj

cd ../Api.Weather.Infrastructure.Tests
dotnet add reference ../../src/Api.Weather.Domain/Api.Weather.Domain.csproj
dotnet add reference ../../src/Api.Weather.Application/Api.Weather.Application.csproj
dotnet add reference ../../src/Api.Weather.Infrastructure/Api.Weather.Infrastructure.csproj

cd ../Api.Weather.Host.Tests
dotnet add reference ../../src/Api.Weather.Domain/Api.Weather.Domain.csproj
dotnet add reference ../../src/Api.Weather.Application/Api.Weather.Application.csproj
dotnet add reference ../../src/Api.Weather.Infrastructure/Api.Weather.Infrastructure.csproj
dotnet add reference ../../src/Api.Weather.Host/Api.Weather.Host.csproj

# Volver al directorio raíz
cd ../..

# Compilar toda la solución
dotnet build
```

---

### Verificar Dependencias

Para verificar las referencias de un proyecto:

```bash
# Ver referencias de un proyecto específico
dotnet list src/Api.Weather.Host/Api.Weather.Host.csproj reference

# Ver todas las referencias en la solución
dotnet sln list
```

---

## Implementación de CQRS sin MediatR

Este proyecto implementa el patrón CQRS (Command Query Responsibility Segregation) **sin usar MediatR**. En su lugar, los servicios se inyectan directamente mediante Dependency Injection.

### Arquitectura de Servicios

```
Controller → Application Service → Domain + Infrastructure
```

### Ejemplo de Implementación

#### 1. **Definir el Command (DTO)**

```csharp
// Application/Commands/RequestForecastCommand.cs
namespace Api.Weather.Application.Commands;

public record RequestForecastCommand(string PostalCode, DateOnly Date);
```

#### 2. **Definir el Query (DTO)**

```csharp
// Application/Queries/GetForecastQuery.cs
namespace Api.Weather.Application.Queries;

public record GetForecastQuery(string PostalCode, DateOnly Date);
```

#### 3. **Implementar el Servicio de Aplicación**

```csharp
// Application/Interfaces/IForecastApplicationService.cs
namespace Api.Weather.Application.Interfaces;

public interface IForecastApplicationService
{
    Task<ForecastDto> RequestForecastAsync(RequestForecastCommand command, CancellationToken cancellationToken = default);
    Task<ForecastDto> GetForecastAsync(GetForecastQuery query, CancellationToken cancellationToken = default);
}

// Application/Services/ForecastApplicationService.cs
namespace Api.Weather.Application.Services;

public class ForecastApplicationService : IForecastApplicationService
{
    private readonly IForecastRepository _repository;
    private readonly IWeatherService _weatherService;
    private readonly IEventPublisher _eventPublisher;
    private readonly IValidator<RequestForecastCommand> _commandValidator;
    private readonly ILogger<ForecastApplicationService> _logger;

    public ForecastApplicationService(
        IForecastRepository repository,
        IWeatherService weatherService,
        IEventPublisher eventPublisher,
        IValidator<RequestForecastCommand> commandValidator,
        ILogger<ForecastApplicationService> logger)
    {
        _repository = repository;
        _weatherService = weatherService;
        _eventPublisher = eventPublisher;
        _commandValidator = commandValidator;
        _logger = logger;
    }

    public async Task<ForecastDto> RequestForecastAsync(
        RequestForecastCommand command, 
        CancellationToken cancellationToken = default)
    {
        // Validar
        var validationResult = await _commandValidator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Crear entidad de dominio
        var postalCode = new PostalCode(command.PostalCode);
        
        // Llamar servicio externo
        var weatherData = await _weatherService.GetForecastAsync(command.PostalCode, command.Date);
        
        // Crear agregado
        var forecast = Forecast.Create(postalCode, command.Date, weatherData);
        
        // Guardar
        await _repository.SaveAsync(forecast);
        
        // Publicar evento
        await _eventPublisher.PublishAsync(new ForecastCreatedEvent(forecast.Id, forecast.PostalCode));
        
        _logger.LogInformation("Forecast created for {PostalCode} on {Date}", command.PostalCode, command.Date);
        
        return MapToDto(forecast);
    }

    public async Task<ForecastDto> GetForecastAsync(
        GetForecastQuery query, 
        CancellationToken cancellationToken = default)
    {
        // Buscar en caché/repositorio
        var forecast = await _repository.GetByPostalCodeAndDateAsync(query.PostalCode, query.Date);
        
        if (forecast == null)
        {
            throw new ForecastNotFoundException($"Forecast not found for {query.PostalCode} on {query.Date}");
        }
        
        return MapToDto(forecast);
    }

    private ForecastDto MapToDto(Forecast forecast)
    {
        return new ForecastDto
        {
            Id = forecast.Id,
            PostalCode = forecast.PostalCode.Value,
            Date = forecast.Date,
            Temperature = forecast.Temperature,
            Humidity = forecast.Humidity,
            Description = forecast.Description
        };
    }
}
```

#### 4. **Usar en el Controller**

```csharp
// Host/Controllers/WeatherController.cs
namespace Api.Weather.Host.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WeatherController : ControllerBase
{
    private readonly IForecastApplicationService _forecastService;
    private readonly ILogger<WeatherController> _logger;

    public WeatherController(
        IForecastApplicationService forecastService,
        ILogger<WeatherController> logger)
    {
        _forecastService = forecastService;
        _logger = logger;
    }

    [HttpPost("forecast")]
    [ProducesResponseType(typeof(ForecastResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestForecast(
        [FromBody] ForecastRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var command = new RequestForecastCommand(request.PostalCode, request.Time);
            var result = await _forecastService.RequestForecastAsync(command, cancellationToken);
            
            return CreatedAtAction(
                nameof(GetForecast), 
                new { postalCode = result.PostalCode, date = result.Date }, 
                result);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { errors = ex.Errors });
        }
    }

    [HttpGet("forecast/{postalCode}/{date}")]
    [ProducesResponseType(typeof(ForecastResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetForecast(
        string postalCode,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        try
        {
            var query = new GetForecastQuery(postalCode, date);
            var result = await _forecastService.GetForecastAsync(query, cancellationToken);
            
            return Ok(result);
        }
        catch (ForecastNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }
}
```

#### 5. **Registrar en Program.cs**

```csharp
// Host/Program.cs
var builder = WebApplication.CreateBuilder(args);

// Registrar servicios de aplicación
builder.Services.AddScoped<IForecastApplicationService, ForecastApplicationService>();

// Registrar validadores de FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<RequestForecastCommandValidator>();

// Otros servicios...
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

### Ventajas de esta Aproximación (sin MediatR)

✅ **Simplicidad**: Menos abstracciones y más directo
✅ **Claridad**: Es obvio qué servicio se está llamando
✅ **Menor curva de aprendizaje**: No requiere entender MediatR
✅ **Menos dependencias**: Una librería menos en el proyecto
✅ **Debugging más fácil**: Stack traces más simples
✅ **Performance**: Sin overhead de reflection de MediatR

### Cuándo considerar MediatR

Considera usar MediatR si necesitas:
- Pipeline behaviors (logging, validación, transacciones automáticas)
- Múltiples handlers para un mismo mensaje
- Desacoplamiento extremo entre capas
- Proyectos muy grandes con muchos commands/queries

---

## Ventajas de DDD

### ✅ Ventajas

1. **Separación de responsabilidades**: Cada capa tiene un propósito claro
2. **Testeable**: El dominio es independiente y fácil de testear
3. **Mantenible**: Cambios en una capa no afectan a otras
4. **Escalable**: Fácil agregar nuevas funcionalidades
5. **Dominio primero**: La lógica de negocio está protegida y centralizada

### ⚠️ Consideraciones

1. **Complejidad inicial**: Más estructura que una arquitectura simple
2. **Overhead**: Para proyectos muy pequeños puede ser excesivo
3. **Curva de aprendizaje**: Requiere entender bien los conceptos

---

## Servicios Meteorológicos Públicos

### 1. **OpenWeatherMap** ⭐ Recomendado
- **URL**: https://openweathermap.org/api
- **Plan gratuito**: 1,000 llamadas/día
- **Formato**: JSON
- **Características**: Predicción actual, 5 días, histórico
- **Endpoint ejemplo**:
  ```
  https://api.openweathermap.org/data/2.5/forecast?zip=28001,ES&appid=API_KEY
  ```

### 2. **AEMET OpenData** (España) 🇪🇸
- **URL**: https://opendata.aemet.es
- **Plan gratuito**: Sí (requiere API key gratuita)
- **Formato**: JSON
- **Características**: Datos oficiales españoles, ideal para códigos postales españoles
- **Endpoint ejemplo**:
  ```
  https://opendata.aemet.es/opendata/api/prediccion/especifica/municipio/diaria/{codigo_municipio}
  ```

### 3. **Open-Meteo** 🆓
- **URL**: https://open-meteo.com
- **Plan gratuito**: Sin límites, sin API key
- **Formato**: JSON
- **Características**: Totalmente gratuito, basado en coordenadas

### 4. **WeatherAPI**
- **URL**: https://www.weatherapi.com
- **Plan gratuito**: 1 millón llamadas/mes
- **Formato**: JSON

**Recomendación**: Para códigos postales españoles usar **AEMET**, para internacional **OpenWeatherMap** u **Open-Meteo**.

---

## Arquitectura de Mensajería (RabbitMQ + MassTransit)

### ¿Por qué usar mensajería?

1. **Desacoplamiento**: Los servicios no necesitan conocerse directamente
2. **Escalabilidad**: Procesamiento asíncrono de tareas pesadas
3. **Resiliencia**: Si un servicio cae, los mensajes se mantienen en la cola
4. **Distribución de carga**: Múltiples consumidores pueden procesar mensajes

### Flujo de Mensajería en Weather API

```
Usuario → API Controller → Publica Mensaje → RabbitMQ
                                                  ↓
                              MassTransit Consumer ← Lee Mensaje
                                                  ↓
                              Procesa (llama servicio externo)
                                                  ↓
                              Guarda en MongoDB → Publica Evento
```

### Ejemplo de Uso

**1. Publicar un evento**:
```csharp
// Application/Interfaces/IEventPublisher.cs
public interface IEventPublisher
{
    Task PublishAsync<T>(T message) where T : class;
}

// Infrastructure/Messaging/Publishers/EventPublisher.cs
public class EventPublisher : IEventPublisher
{
    private readonly IPublishEndpoint _publishEndpoint;
    
    public async Task PublishAsync<T>(T message) where T : class
    {
        await _publishEndpoint.Publish(message);
    }
}
```

**2. Consumir un mensaje**:
```csharp
// Infrastructure/Messaging/Consumers/ForecastRequestedConsumer.cs
public class ForecastRequestedConsumer : IConsumer<ForecastRequestedMessage>
{
    private readonly IWeatherService _weatherService;
    private readonly IForecastRepository _repository;
    
    public async Task Consume(ConsumeContext<ForecastRequestedMessage> context)
    {
        var forecast = await _weatherService.GetForecastAsync(
            context.Message.PostalCode,
            context.Message.Date
        );
        
        await _repository.SaveAsync(forecast);
        
        await context.Publish(new ForecastCreatedMessage { Id = forecast.Id });
    }
}
```

**3. Configuración en Program.cs**:
```csharp
// Registrar MassTransit con RabbitMQ
builder.Services.AddMassTransit(config =>
{
    // Registrar consumidores
    config.AddConsumer<ForecastRequestedConsumer>();
    config.AddConsumer<WeatherDataUpdatedConsumer>();
    
    // Configurar RabbitMQ
    config.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
        
        // Configurar endpoints
        cfg.ConfigureEndpoints(context);
    });
});
```

### Ventajas de MassTransit

- ✅ Abstracción sobre RabbitMQ (facilita cambiar a Azure Service Bus, etc.)
- ✅ Manejo automático de retry y error handling
- ✅ Soporte para sagas y orquestación
- ✅ Serialización automática
- ✅ Inyección de dependencias integrada

---

## Persistencia con MongoDB

### ¿Por qué MongoDB para este proyecto?

1. **Flexibilidad**: Esquema flexible para diferentes tipos de pronósticos
2. **Rendimiento**: Excelente para operaciones de lectura
3. **Escalabilidad horizontal**: Sharding nativo
4. **Documentos JSON**: Mapeo natural con objetos C#

### Configuración MongoDB

**1. Contexto MongoDB**:
```csharp
// Infrastructure/Persistence/Context/MongoDbContext.cs
public class MongoDbContext
{
    private readonly IMongoDatabase _database;
    
    public MongoDbContext(IOptions<MongoDbSettings> settings)
    {
        var client = new MongoClient(settings.Value.ConnectionString);
        _database = client.GetDatabase(settings.Value.DatabaseName);
    }
    
    public IMongoCollection<Forecast> Forecasts => 
        _database.GetCollection<Forecast>("forecasts");
    
    public IMongoCollection<WeatherAlert> Alerts => 
        _database.GetCollection<WeatherAlert>("alerts");
}
```

**2. Configuración de índices**:
```csharp
// Infrastructure/Persistence/Configurations/IndexConfiguration.cs
public static class IndexConfiguration
{
    public static void ConfigureIndexes(IMongoDatabase database)
    {
        var forecasts = database.GetCollection<Forecast>("forecasts");
        
        // Índice compuesto para búsquedas por código postal y fecha
        var indexKeys = Builders<Forecast>.IndexKeys
            .Ascending(f => f.PostalCode)
            .Ascending(f => f.Date);
            
        forecasts.Indexes.CreateOne(new CreateIndexModel<Forecast>(indexKeys));
        
        // Índice TTL para expiración automática (opcional)
        var ttlIndex = Builders<Forecast>.IndexKeys.Ascending(f => f.CreatedAt);
        var ttlOptions = new CreateIndexOptions { ExpireAfter = TimeSpan.FromDays(30) };
        forecasts.Indexes.CreateOne(new CreateIndexModel<Forecast>(ttlIndex, ttlOptions));
    }
}
```

**3. Repositorio con MongoDB**:
```csharp
// Infrastructure/Persistence/Repositories/ForecastRepository.cs
public class ForecastRepository : IForecastRepository
{
    private readonly IMongoCollection<Forecast> _forecasts;
    
    public async Task<Forecast?> GetByPostalCodeAndDateAsync(
        string postalCode, 
        DateOnly date)
    {
        return await _forecasts
            .Find(f => f.PostalCode == postalCode && f.Date == date)
            .FirstOrDefaultAsync();
    }
    
    public async Task SaveAsync(Forecast forecast)
    {
        await _forecasts.ReplaceOneAsync(
            f => f.Id == forecast.Id,
            forecast,
            new ReplaceOptions { IsUpsert = true }
        );
    }
}
```

**4. Configuración en appsettings.json**:
```json
{
  "MongoDbSettings": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "weather_db"
  },
  "RabbitMqSettings": {
    "Host": "localhost",
    "Username": "guest",
    "Password": "guest"
  }
}
```

---

## Patrones Utilizados

### Arquitectura y Diseño
- **Repository Pattern**: Abstracción del acceso a datos
- **CQRS** (Command Query Responsibility Segregation): Separación lectura/escritura
- **Dependency Injection**: Inversión de control
- **Value Objects**: Objetos inmutables con validación
- **Domain Events**: Comunicación desacoplada

### Mensajería y Comunicación
- **Publish/Subscribe**: Publicación de eventos con múltiples suscriptores
- **Message Consumer**: Consumidores de mensajes con MassTransit
- **Event-Driven Architecture**: Arquitectura basada en eventos
- **Outbox Pattern** (opcional): Garantiza consistencia entre DB y mensajería

### Persistencia
- **Unit of Work** (MongoDB): Transacciones y consistencia
- **Document Store Pattern**: Almacenamiento de documentos NoSQL
- **Index Strategy**: Optimización de consultas con índices

### Resiliencia
- **Retry Pattern**: Reintentos con Polly
- **Circuit Breaker**: Protección contra fallos en cascada
- **Timeout Pattern**: Límites de tiempo en operaciones externas

---

## Dependencias y Paquetes NuGet

### Domain
```xml
<!-- Sin dependencias externas, solo .NET 8 -->
```

### Application
```xml
<PackageReference Include="FluentValidation" Version="11.9.0" />
```

### Infrastructure
```xml
<!-- MongoDB -->
<PackageReference Include="MongoDB.Driver" Version="2.23.1" />

<!-- MassTransit y RabbitMQ -->
<PackageReference Include="MassTransit" Version="8.1.3" />
<PackageReference Include="MassTransit.RabbitMQ" Version="8.1.3" />

<!-- HttpClient y Resiliencia -->
<PackageReference Include="Microsoft.Extensions.Http" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Http.Polly" Version="8.0.0" />
<PackageReference Include="Polly" Version="8.2.0" />

<!-- Caché (opcional) -->
<PackageReference Include="StackExchange.Redis" Version="2.7.10" />
```

### Host/API
```xml
<PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
<PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="12.0.1" />
<PackageReference Include="Serilog.AspNetCore" Version="8.0.0" />
```

### Tests
```xml
<PackageReference Include="xUnit" Version="2.6.4" />
<PackageReference Include="NSubstitute" Version="5.1.0" />
<PackageReference Include="AwesomeAssertions" Version="9.3.0" />
<PackageReference Include="Testcontainers.MongoDb" Version="3.6.0" />
<PackageReference Include="Testcontainers.RabbitMq" Version="3.6.0" />
```

**Nota sobre librerías de testing**:
- **NSubstitute**: Framework de mocking más simple y expresivo que Moq
- **AwesomeAssertions 9.3.0**: Librería de aserciones fluidas para escribir tests más legibles y expresivos

---

## Ejemplos de Testing

### NSubstitute - Mocking

**Crear un mock**:
```csharp
// Crear un mock de una interfaz
var weatherService = Substitute.For<IWeatherService>();

// Configurar comportamiento
weatherService
    .GetForecastAsync("28001", Arg.Any<DateOnly>())
    .Returns(new Forecast { Temperature = 25 });

// Verificar que se llamó
await weatherService.Received(1).GetForecastAsync("28001", Arg.Any<DateOnly>());

// Verificar que NO se llamó
weatherService.DidNotReceive().GetHistoricalData(Arg.Any<string>());
```

**Ejemplo completo de test**:
```csharp
public class ForecastServiceTests
{
    private readonly IWeatherService _weatherService;
    private readonly IForecastRepository _repository;
    private readonly ForecastService _sut;

    public ForecastServiceTests()
    {
        _weatherService = Substitute.For<IWeatherService>();
        _repository = Substitute.For<IForecastRepository>();
        _sut = new ForecastService(_weatherService, _repository);
    }

    [Fact]
    public async Task GetForecast_ShouldReturnFromCache_WhenExists()
    {
        // Arrange
        var expectedForecast = new Forecast 
        { 
            PostalCode = "28001", 
            Temperature = 25 
        };
        
        _repository
            .GetByPostalCodeAndDateAsync("28001", Arg.Any<DateOnly>())
            .Returns(expectedForecast);

        // Act
        var result = await _sut.GetForecastAsync("28001", DateOnly.FromDateTime(DateTime.Today));

        // Assert
        result.Should().NotBeNull();
        result.Temperature.Should().Be(25);
        
        // Verificar que NO se llamó al servicio externo
        await _weatherService.DidNotReceive().GetForecastAsync(Arg.Any<string>(), Arg.Any<DateOnly>());
    }
}
```

### AwesomeAssertions 9.3.0 - Aserciones

**Aserciones básicas**:
```csharp
// Objetos
forecast.Should().NotBeNull();
forecast.Should().BeOfType<Forecast>();

// Strings
postalCode.Should().Be("28001");
postalCode.Should().StartWith("280");
postalCode.Should().HaveLength(5);

// Números
temperature.Should().BeGreaterThan(0);
temperature.Should().BeInRange(-10, 50);

// Colecciones
forecasts.Should().NotBeEmpty();
forecasts.Should().HaveCount(7);
forecasts.Should().Contain(f => f.PostalCode == "28001");
forecasts.Should().OnlyContain(f => f.Temperature > 0);

// Excepciones
var act = async () => await service.GetForecastAsync(null, DateOnly.Today);
await act.Should().ThrowAsync<ArgumentNullException>()
    .WithMessage("*postalCode*");

// Fechas
createdAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
date.Should().BeOnOrAfter(DateOnly.FromDateTime(DateTime.Today));
```

**Aserciones de objetos complejos**:
```csharp
forecast.Should().BeEquivalentTo(new 
{
    PostalCode = "28001",
    Temperature = 25,
    Humidity = 60
}, options => options.ExcludingMissingMembers());

// Comparar propiedades específicas
forecast.Should().BeEquivalentTo(expectedForecast, options => options
    .Including(f => f.PostalCode)
    .Including(f => f.Temperature)
    .Excluding(f => f.CreatedAt));
```

### Estructura de un Test Completo

```csharp
public class GetForecastQueryHandlerTests
{
    private readonly IForecastRepository _repository;
    private readonly IWeatherService _weatherService;
    private readonly IEventPublisher _eventPublisher;
    private readonly GetForecastQueryHandler _handler;

    public GetForecastQueryHandlerTests()
    {
        _repository = Substitute.For<IForecastRepository>();
        _weatherService = Substitute.For<IWeatherService>();
        _eventPublisher = Substitute.For<IEventPublisher>();
        _handler = new GetForecastQueryHandler(_repository, _weatherService, _eventPublisher);
    }

    [Fact]
    public async Task Handle_ShouldReturnCachedForecast_WhenExists()
    {
        // Arrange
        var query = new GetForecastQuery("28001", DateOnly.FromDateTime(DateTime.Today));
        var cachedForecast = new Forecast
        {
            Id = Guid.NewGuid(),
            PostalCode = "28001",
            Temperature = 25,
            Humidity = 60,
            Date = DateOnly.FromDateTime(DateTime.Today)
        };

        _repository
            .GetByPostalCodeAndDateAsync("28001", query.Date)
            .Returns(cachedForecast);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(new
        {
            PostalCode = "28001",
            Temperature = 25,
            Humidity = 60
        });

        // Verificaciones
        await _repository.Received(1).GetByPostalCodeAndDateAsync("28001", query.Date);
        await _weatherService.DidNotReceive().GetForecastAsync(Arg.Any<string>(), Arg.Any<DateOnly>());
        await _eventPublisher.DidNotReceive().PublishAsync(Arg.Any<object>());
    }

    [Fact]
    public async Task Handle_ShouldCallExternalService_WhenNotInCache()
    {
        // Arrange
        var query = new GetForecastQuery("28001", DateOnly.FromDateTime(DateTime.Today));
        var externalForecast = new Forecast
        {
            Id = Guid.NewGuid(),
            PostalCode = "28001",
            Temperature = 22,
            Date = query.Date
        };

        _repository
            .GetByPostalCodeAndDateAsync("28001", query.Date)
            .Returns((Forecast?)null);

        _weatherService
            .GetForecastAsync("28001", query.Date)
            .Returns(externalForecast);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Temperature.Should().Be(22);

        await _weatherService.Received(1).GetForecastAsync("28001", query.Date);
        await _repository.Received(1).SaveAsync(Arg.Is<Forecast>(f => 
            f.PostalCode == "28001" && f.Temperature == 22));
        await _eventPublisher.Received(1).PublishAsync(Arg.Any<ForecastCreatedEvent>());
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("123")]
    public async Task Handle_ShouldThrowException_WhenInvalidPostalCode(string invalidPostalCode)
    {
        // Arrange
        var query = new GetForecastQuery(invalidPostalCode, DateOnly.FromDateTime(DateTime.Today));

        // Act
        var act = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidPostalCodeException>()
            .WithMessage("*postal code*");
    }
}
```

### Tests de Integración con Testcontainers

```csharp
public class ForecastRepositoryIntegrationTests : IAsyncLifetime
{
    private MongoDbContainer _mongoContainer;
    private IMongoDatabase _database;
    private ForecastRepository _repository;

    public async Task InitializeAsync()
    {
        _mongoContainer = new MongoDbBuilder()
            .WithImage("mongo:7.0")
            .Build();

        await _mongoContainer.StartAsync();

        var client = new MongoClient(_mongoContainer.GetConnectionString());
        _database = client.GetDatabase("test_weather_db");
        _repository = new ForecastRepository(_database);
    }

    [Fact]
    public async Task SaveAsync_ShouldPersistForecast()
    {
        // Arrange
        var forecast = new Forecast
        {
            Id = Guid.NewGuid(),
            PostalCode = "28001",
            Temperature = 25,
            Date = DateOnly.FromDateTime(DateTime.Today)
        };

        // Act
        await _repository.SaveAsync(forecast);
        var result = await _repository.GetByPostalCodeAndDateAsync("28001", forecast.Date);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(forecast);
    }

    public async Task DisposeAsync()
    {
        await _mongoContainer.DisposeAsync();
    }
}
```

---

## Referencias

- [Domain-Driven Design (Eric Evans)](https://www.domainlanguage.com/ddd/)
- [Clean Architecture (Robert C. Martin)](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Microsoft - DDD in .NET](https://docs.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/)

---

**Fecha de creación**: 2025-12-23

