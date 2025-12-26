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

### 🗄️ Base de Datos
- **[../MONGODB_CONFIG.md](../MONGODB_CONFIG.md)** - Configuración completa de MongoDB
  - Instalación local y con Docker
  - Configuración del proyecto
  - Comandos útiles
  - Solución de problemas

### 🎯 Patrones de Diseño
- **[../OPTIONS_PATTERN.md](../OPTIONS_PATTERN.md)** - Documentación del Options Pattern
  - Implementación en el proyecto
  - Ventajas del patrón
  - Ejemplos de uso
  - Extensiones futuras

### 📝 Otros Recursos
- **[../README.md](../README.md)** - README principal del proyecto (raíz)
- **[../SETUP_SUMMARY.md](../SETUP_SUMMARY.md)** - Resumen de configuración de MongoDB

---

## 🎯 Guías por Rol

### 👨‍💻 Desarrollador Nuevo
**Comienza aquí**:
1. [README.md](README.md) → Sección "Descripción General"
2. [README.md](README.md) → Sección "Guía de Configuración y Ejecución"
3. [OPENCAGE_API_SETUP.md](OPENCAGE_API_SETUP.md) → Obtener API Key
4. [../MONGODB_CONFIG.md](../MONGODB_CONFIG.md) → Configurar MongoDB
5. [README.md](README.md) → Sección "Probar la API"

### 🏗️ Arquitecto de Software
**Enfócate en**:
1. [README.md](README.md) → Sección "Arquitectura del Proyecto Actual"
2. [README.md](README.md) → Sección "Descripción de Capas"
3. [README.md](README.md) → Sección "Flujo de Dependencias"
4. [README.md](README.md) → Sección "Patrones y Principios Aplicados"
5. [../OPTIONS_PATTERN.md](../OPTIONS_PATTERN.md) → Patrón Options

### 🔧 DevOps / SRE
**Consulta**:
1. [../MONGODB_CONFIG.md](../MONGODB_CONFIG.md) → Instalación y configuración
2. [README.md](README.md) → Sección "MongoDB - Persistencia de Datos"
3. [README.md](README.md) → Sección "Solución de Problemas Comunes"
4. Docker Compose: `../.docker/docker-compose.yaml`
5. Script de gestión: `../mongodb.sh`

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
| ../MONGODB_CONFIG.md | ✅ Actualizado | 2025-12-26 | ~214 |
| ../OPTIONS_PATTERN.md | ✅ Actualizado | 2025-12-26 | ~180 |
| ../README.md | ⚠️ Por sincronizar | - | - |

---

## 🔍 Búsqueda Rápida por Tema

### Configuración
- **MongoDB**: [README.md](README.md#-mongodb---persistencia-de-datos) | [../MONGODB_CONFIG.md](../MONGODB_CONFIG.md)
- **Options Pattern**: [README.md](README.md#configuración-con-options-pattern) | [../OPTIONS_PATTERN.md](../OPTIONS_PATTERN.md)
- **API Keys**: [OPENCAGE_API_SETUP.md](OPENCAGE_API_SETUP.md)
- **Docker**: [../MONGODB_CONFIG.md](../MONGODB_CONFIG.md) | `../.docker/`

### Arquitectura
- **Estructura del Proyecto**: [README.md](README.md#-arquitectura-del-proyecto-actual)
- **Capas**: [README.md](README.md#-descripción-de-capas)
- **Dependencias**: [README.md](README.md#-flujo-de-dependencias)
- **Patrones**: [README.md](README.md#-patrones-y-principios-aplicados)

### APIs y Servicios
- **OpenCage API**: [OPENCAGE_API_SETUP.md](OPENCAGE_API_SETUP.md)
- **Open-Meteo API**: [OPEN_METEO_SERVICE.md](OPEN_METEO_SERVICE.md)
- **Endpoints**: [README.md](README.md#-apiweatherhost-presentación)

### Desarrollo
- **Ejecución**: [README.md](README.md#-guía-de-configuración-y-ejecución)
- **Testing**: [README.md](README.md#7-probar-la-api)
- **Troubleshooting**: [README.md](README.md#-solución-de-problemas-comunes)

### Base de Datos
- **MongoDB Setup**: [../MONGODB_CONFIG.md](../MONGODB_CONFIG.md)
- **Comandos**: [README.md](README.md#comandos-útiles-de-mongodb)
- **Estructura**: [README.md](README.md#estructura-de-la-base-de-datos)

---

## 🚀 Inicio Rápido

### Para ejecutar el proyecto en 5 minutos:

1. **Instalar MongoDB**:
   ```bash
   cd .docker
   docker compose --profile infrastructure up -d
   ```

2. **Configurar API Key**:
   - Obtén una key gratuita en https://opencagedata.com/
   - Edita `src/Api.Weather.Host/appsettings.Development.json`

3. **Ejecutar**:
   ```bash
   dotnet run --project src/Api.Weather.Host/
   ```

4. **Probar**:
   - Abre https://localhost:5001/swagger

Ver guía completa: [README.md](README.md#-guía-de-configuración-y-ejecución)

---

## 📞 Soporte

- **Problemas comunes**: [README.md](README.md#-solución-de-problemas-comunes)
- **MongoDB issues**: [../MONGODB_CONFIG.md](../MONGODB_CONFIG.md#solución-de-problemas)
- **OpenCage API**: [OPENCAGE_API_SETUP.md](OPENCAGE_API_SETUP.md#verificación)

---

## 📝 Notas

- Todos los archivos están en formato Markdown
- Los enlaces relativos funcionan en GitHub y editores locales
- La documentación refleja el estado actual del proyecto (2025-12-26)
- Para contribuir, consulta [README.md](README.md#-contribuir)

---

**Última actualización**: 2025-12-26
**Mantenedor**: Equipo Weather API

