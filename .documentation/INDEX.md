# 📚 Índice de Documentación - Weather API

## 📍 Navegación Rápida

### 📖 Documentación Principal
- **[README.md](README.md)** - Documentación completa del proyecto (👈 Empieza aquí)
  - Arquitectura actual del proyecto
  - Guía de configuración paso a paso
  - Descripción de capas y dependencias
  - Patrones y principios aplicados
  - Solución de problemas comunes

### 🔧 Configuración de Servicios Externos
- **[OPENCAGE_API_SETUP.md](OPENCAGE_API_SETUP.md)** - Configuración de OpenCage Geocoding API
  - Cómo obtener API Key gratuita (2,500 peticiones/día)
  - Configuración en el proyecto
  - Ejemplos de uso
  - Solución de problemas

- **[OPEN_METEO_SERVICE.md](OPEN_METEO_SERVICE.md)** - Documentación de Open-Meteo Weather API
  - API meteorológica gratuita e ilimitada
  - No requiere API Key
  - Datos disponibles
  - Ejemplos de integración

### 📊 Observabilidad y Monitoreo
- **[OPENTELEMETRY_QUICKSTART.md](OPENTELEMETRY_QUICKSTART.md)** - Guía rápida de OpenTelemetry (👈 Empieza aquí)
  - Inicio en 5 minutos
  - Comandos útiles
  - Acceso a servicios
  - Troubleshooting básico

- **[OPENTELEMETRY_CONFIGURATION.md](OPENTELEMETRY_CONFIGURATION.md)** - Documentación completa de OpenTelemetry
  - Configuración detallada
  - Trazas personalizadas con ActivitySource
  - Integración con Grafana Stack (Tempo, Loki, Prometheus)
  - Mejores prácticas
  - Ejemplos avanzados

### 🏛️ Arquitectura y Diseño
- **[DDD_ANALYSIS.md](DDD_ANALYSIS.md)** - Análisis de Domain-Driven Design
  - Estado actual del proyecto (57.8% adherencia a DDD)
  - Aspectos correctos e incorrectos
  - Plan de acción priorizado
  - Ejemplos de código
  - Recomendaciones

- **[PROBLEMDETAILS_IMPLEMENTATION.md](PROBLEMDETAILS_IMPLEMENTATION.md)** - Implementación de ProblemDetails RFC 7807
  - Global Exception Handler
  - Integración con OpenTelemetry (TraceId)
  - Errores personalizados

- **[API_VERSIONING.md](API_VERSIONING.md)** - Versionado de API
  - Configuración de API Versioning
  - Versionado en URL

### 🗄️ Base de Datos y Infraestructura
- **Docker Compose**: `../.docker/docker-compose.yaml` - Configuración completa
  - MongoDB con replica set
  - Stack Grafana (Tempo, Loki, Prometheus, OTEL Collector)
- **Variables de entorno**: `../.docker/.env` - Variables de configuración

---

## 🎯 Guías por Rol

### 👨‍💻 Desarrollador Nuevo
**Comienza aquí**:
1. [README.md](README.md) → Sección "Descripción General"
2. [README.md](README.md) → Sección "Guía de Configuración y Ejecución"
3. [OPENCAGE_API_SETUP.md](OPENCAGE_API_SETUP.md) → Obtener API Key
4. [OPENTELEMETRY_QUICKSTART.md](OPENTELEMETRY_QUICKSTART.md) → Configurar observabilidad
5. [README.md](README.md) → Sección "Probar la API"

### 🏗️ Arquitecto de Software
**Enfócate en**:
1. [README.md](README.md) → Sección "Arquitectura del Proyecto Actual"
2. [DDD_ANALYSIS.md](DDD_ANALYSIS.md) → Análisis de DDD completo
3. [README.md](README.md) → Sección "Patrones y Principios Aplicados"
4. [PROBLEMDETAILS_IMPLEMENTATION.md](PROBLEMDETAILS_IMPLEMENTATION.md) → Error handling
5. [API_VERSIONING.md](API_VERSIONING.md) → Estrategia de versionado

### 🔧 DevOps / SRE
**Consulta**:
1. [OPENTELEMETRY_QUICKSTART.md](OPENTELEMETRY_QUICKSTART.md) → Stack de observabilidad
2. [OPENTELEMETRY_CONFIGURATION.md](OPENTELEMETRY_CONFIGURATION.md) → Configuración avanzada
3. [README.md](README.md) → Sección "Configurar MongoDB"
4. Docker Compose: `../.docker/docker-compose.yaml`
5. [README.md](README.md) → Sección "Solución de Problemas Comunes"

### 🧪 QA / Tester
**Revisa**:
1. [README.md](README.md) → Sección "Probar la API"
2. [OPEN_METEO_SERVICE.md](OPEN_METEO_SERVICE.md) → Datos disponibles
3. [README.md](README.md) → Sección "Solución de Problemas Comunes"
4. Swagger UI: https://localhost:5001/swagger

---

## 📊 Estado de la Documentación

| Archivo | Estado | Última Actualización | Líneas |
|---------|--------|---------------------|--------|
| README.md | ✅ Actualizado | 2025-12-26 | ~870 |
| OPENCAGE_API_SETUP.md | ✅ Actualizado | 2025-12-23 | ~407 |
| OPEN_METEO_SERVICE.md | ✅ Actualizado | 2025-12-23 | ~379 |
| OPENTELEMETRY_QUICKSTART.md | ✅ Nuevo | 2025-12-27 | ~200 |
| OPENTELEMETRY_CONFIGURATION.md | ✅ Nuevo | 2025-12-27 | ~550 |
| DDD_ANALYSIS.md | ✅ Actualizado | 2025-12-27 | ~1,784 |
| PROBLEMDETAILS_IMPLEMENTATION.md | ✅ Actualizado | 2025-12-26 | ~250 |
| API_VERSIONING.md | ✅ Actualizado | 2025-12-26 | ~180 |
| INDEX.md | ✅ Actualizado | 2025-12-27 | ~200 |

---

## 🔍 Búsqueda Rápida por Tema

### Configuración
- **MongoDB**: [README.md](README.md#-mongodb---persistencia-de-datos)
- **OpenTelemetry**: [OPENTELEMETRY_QUICKSTART.md](OPENTELEMETRY_QUICKSTART.md)
- **Options Pattern**: [README.md](README.md#configuración-con-options-pattern)
- **API Keys**: [OPENCAGE_API_SETUP.md](OPENCAGE_API_SETUP.md)
- **Docker**: `../.docker/docker-compose.yaml` | `../.docker/.env`

### Observabilidad y Monitoreo
- **Inicio Rápido**: [OPENTELEMETRY_QUICKSTART.md](OPENTELEMETRY_QUICKSTART.md)
- **Configuración Completa**: [OPENTELEMETRY_CONFIGURATION.md](OPENTELEMETRY_CONFIGURATION.md)
- **Trazas Distribuidas**: [OPENTELEMETRY_CONFIGURATION.md](OPENTELEMETRY_CONFIGURATION.md#-uso-avanzado-trazas-personalizadas)
- **Métricas**: [OPENTELEMETRY_CONFIGURATION.md](OPENTELEMETRY_CONFIGURATION.md#-métricas-disponibles)
- **Grafana Stack**: [OPENTELEMETRY_QUICKSTART.md](OPENTELEMETRY_QUICKSTART.md#-acceso-rápido-a-los-servicios)

### Arquitectura
- **Estructura del Proyecto**: [README.md](README.md#-arquitectura-del-proyecto-actual)
- **Capas**: [README.md](README.md#-descripción-de-capas)
- **Dependencias**: [README.md](README.md#-flujo-de-dependencias)
- **Patrones**: [README.md](README.md#-patrones-y-principios-aplicados)
- **Análisis DDD**: [DDD_ANALYSIS.md](DDD_ANALYSIS.md)

### APIs y Servicios
- **OpenCage API**: [OPENCAGE_API_SETUP.md](OPENCAGE_API_SETUP.md)
- **Open-Meteo API**: [OPEN_METEO_SERVICE.md](OPEN_METEO_SERVICE.md)
- **Endpoints**: [README.md](README.md#-apiweatherhost-presentación)
- **ProblemDetails**: [PROBLEMDETAILS_IMPLEMENTATION.md](PROBLEMDETAILS_IMPLEMENTATION.md)
- **API Versioning**: [API_VERSIONING.md](API_VERSIONING.md)

### Desarrollo
- **Ejecución**: [README.md](README.md#-guía-de-configuración-y-ejecución)
- **Testing**: [README.md](README.md#7-probar-la-api)
- **Troubleshooting**: [README.md](README.md#-solución-de-problemas-comunes)
- **Debugging con Trazas**: [OPENTELEMETRY_CONFIGURATION.md](OPENTELEMETRY_CONFIGURATION.md#5-correlacionar-errores-con-trazas)

### Base de Datos
- **MongoDB Setup**: [README.md](README.md#-configurar-mongodb)
- **Comandos**: [README.md](README.md#comandos-útiles-de-mongodb)
- **Estructura**: [README.md](README.md#estructura-de-la-base-de-datos)
- **Docker Compose**: `../.docker/docker-compose.yaml`

---

## 🚀 Inicio Rápido

### Para ejecutar el proyecto en 5 minutos:

1. **Instalar MongoDB**:
   ```bash
   cd .docker
   docker compose --profile infrastructure up -d
   ```

2. **(Opcional) Iniciar Stack de Observabilidad**:
   ```bash
   cd .docker
   docker compose --profile infrastructure-monitoring up -d
   ```

3. **Configurar API Key**:
   - Obtén una key gratuita en https://opencagedata.com/
   - Edita `src/Api.Weather.Host/appsettings.Development.json`

4. **Ejecutar**:
   ```bash
   dotnet run --project src/Api.Weather.Host/
   ```

5. **Probar**:
   - API: https://localhost:5001/swagger
   - Grafana (si iniciaste monitoring): http://localhost:3000

Ver guías completas:
- [README.md](README.md#-guía-de-configuración-y-ejecución)
- [OPENTELEMETRY_QUICKSTART.md](OPENTELEMETRY_QUICKSTART.md)

---

## 📞 Soporte

- **Problemas comunes**: [README.md](README.md#-solución-de-problemas-comunes)
- **MongoDB**: [README.md](README.md#-mongodb---persistencia-de-datos)
- **OpenCage API**: [OPENCAGE_API_SETUP.md](OPENCAGE_API_SETUP.md#verificación)

---

## 📝 Notas

- Todos los archivos están en formato Markdown
- Los enlaces relativos funcionan en GitHub y editores locales
- La documentación refleja el estado actual del proyecto (2025-12-26)
- Para contribuir, consulta [README.md](README.md#-contribuir)

---

**Última actualización**: 2025-12-27  
**Mantenedor**: Equipo Weather API

