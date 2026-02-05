# Configuración de OpenCage Geocoding API

## ¿Por qué OpenCage?

OpenCage es una excelente alternativa a Google Maps para geocodificación con estas ventajas:

- ✅ **Cuota gratuita generosa**: 2,500 peticiones/día gratis
- ✅ **Basada en OpenStreetMap**: Datos de alta calidad
- ✅ **Sin tarjeta de crédito**: No necesitas ingresar tarjeta para empezar
- ✅ **Fácil de usar**: API simple y directa
- ✅ **Sin restricciones**: No necesitas configurar IPs o dominios

---

## Pasos para obtener tu API Key

### 1. Regístrate en OpenCage

1. Ve a [OpenCage Geocoding API](https://opencagedata.com/)
2. Haz clic en **"Sign Up"** o **"Get Your API Key"**
3. Completa el formulario de registro (nombre, email, contraseña)
4. Confirma tu email

### 2. Obtén tu API Key

1. Inicia sesión en [OpenCage Dashboard](https://opencagedata.com/dashboard)
2. En el dashboard verás tu API Key inmediatamente
3. Copia la API Key (algo como: `a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6`)

### 3. Verifica tu Cuota

En el dashboard podrás ver:
- **Peticiones usadas hoy**
- **Peticiones restantes**
- **Plan actual** (Free: 2,500/día)

---

## Configuración en el Proyecto

### Desarrollo Local

1. Abre el archivo `appsettings.Development.json`
2. Reemplaza `YOUR_OPENCAGE_API_KEY_HERE` con tu API Key:

```json
{
  "OpenCage": {
    "ApiKey": "a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6"
  }
}
```

### Producción

**⚠️ IMPORTANTE: NUNCA subas tu API Key al control de versiones**

#### Opción 1: Variables de Entorno (Recomendado)

```bash
export OpenCage__ApiKey="a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6"
```

#### Opción 2: Azure App Service Configuration

1. Ve a tu App Service en Azure Portal
2. En "Configuración" > "Configuración de la aplicación"
3. Añade una nueva configuración:
   - **Nombre**: `OpenCage:ApiKey`
   - **Valor**: Tu API Key

#### Opción 3: Docker Secrets

```yaml
version: '3.8'
services:
  weatherapi:
    image: weatherapi:latest
    environment:
      - OpenCage__ApiKey=${OPENCAGE_API_KEY}
```

#### Opción 4: Kubernetes Secret

```yaml
apiVersion: v1
kind: Secret
metadata:
  name: opencage-secret
type: Opaque
stringData:
  apiKey: a1b2c3d4e5f6g7h8i9j0k1l2m3n4o5p6
```

---

## Verificación

### Probar la API Key manualmente

Abre esta URL en tu navegador (reemplaza con tu API Key):

```
https://api.opencagedata.com/geocode/v1/json?q=28001+Spain&key=TU_API_KEY&limit=1&no_annotations=1
```

**Respuesta exitosa**:
```json
{
  "results": [
    {
      "geometry": {
        "lat": 40.4168,
        "lng": -3.7038
      },
      "formatted": "28001 Madrid, España"
    }
  ],
  "status": {
    "code": 200,
    "message": "OK"
  },
  "rate": {
    "limit": 2500,
    "remaining": 2499,
    "reset": 1703462400
  }
}
```

### Probar en el proyecto

```bash
# Ejecutar el proyecto
dotnet run --project src/Api.Weather.Host

# O con Docker
docker-compose up
```

---

## Cuotas y Límites

### Plan Gratuito

- ✅ **2,500 peticiones/día** (reset a medianoche UTC)
- ✅ Sin tarjeta de crédito requerida
- ✅ Acceso a todas las funciones
- ✅ Sin expiración

### Planes de Pago

Si necesitas más peticiones:

| Plan | Peticiones/día | Precio/mes |
|------|----------------|------------|
| **Free** | 2,500 | $0 |
| **Trial** | 10,000 | $50 |
| **Small** | 25,000 | $100 |
| **Medium** | 100,000 | $300 |
| **Large** | 500,000 | $1,000 |

---

## Características de la API

### Parámetros Soportados

```
GET https://api.opencagedata.com/geocode/v1/json?q={query}&key={API_KEY}
```

**Parámetros principales**:
- `q`: Consulta (código postal, dirección, coordenadas)
- `key`: Tu API Key (requerido)
- `limit`: Número de resultados (default: 10, recomendado: 1)
- `no_annotations`: Omite datos extra para respuestas más rápidas (recomendado: 1)
- `language`: Idioma de resultados (ej: `es`, `en`)
- `countrycode`: Filtrar por país (ej: `es`, `fr`)

### Ejemplo de Respuesta Completa

```json
{
  "results": [
    {
      "geometry": {
        "lat": 40.4167754,
        "lng": -3.7037902
      },
      "components": {
        "postcode": "28001",
        "country": "España",
        "country_code": "es"
      },
      "formatted": "28001 Madrid, España"
    }
  ],
  "status": {
    "code": 200,
    "message": "OK"
  },
  "rate": {
    "limit": 2500,
    "remaining": 2499,
    "reset": 1703462400
  }
}
```

### Códigos de Estado

- **200**: OK - Petición exitosa
- **400**: Invalid request - Parámetros incorrectos
- **402**: Quota exceeded - Excediste tu cuota
- **403**: Invalid API key - API key inválida o suspendida
- **404**: Not found - No se encontraron resultados
- **429**: Too many requests - Rate limit excedido (1 req/seg)
- **500**: Server error - Error interno del servidor

---

## Rate Limiting

OpenCage tiene límites de velocidad:

- **1 petición por segundo** en el plan gratuito
- **15 peticiones por segundo** en planes de pago

### Implementar Rate Limiting

```csharp
services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("opencage", opt =>
    {
        opt.Window = TimeSpan.FromSeconds(1);
        opt.PermitLimit = 1; // 1 petición por segundo (plan gratuito)
    });
});
```

---

## Seguridad - Mejores Prácticas

### ✅ Hacer

1. **Usar variables de entorno** en producción
2. **No hardcodear la API Key** en el código
3. **Añadir la API Key a .gitignore**
4. **Monitorear el uso** en el dashboard
5. **Implementar caché** para códigos postales frecuentes
6. **Respetar rate limits** (1 req/seg)

### ❌ No hacer

1. ❌ Subir la API Key al repositorio
2. ❌ Compartir la API Key públicamente
3. ❌ Usar la API Key en código frontend
4. ❌ Exceder los rate limits

---

## Caché Recomendado

Para optimizar y reducir peticiones:

```csharp
services.AddMemoryCache();

public class CachedGeolocationService : IGeolocationService
{
    private readonly IGeolocationService _innerService;
    private readonly IMemoryCache _cache;

    public (string lat, string lon) GetCoordinates(string postalCode)
    {
        var cacheKey = $"geo_{postalCode}";
        
        if (!_cache.TryGetValue(cacheKey, out (string, string) coordinates))
        {
            coordinates = _innerService.GetCoordinates(postalCode);
            
            // Cachear por 30 días (códigos postales no cambian)
            _cache.Set(cacheKey, coordinates, TimeSpan.FromDays(30));
        }
        
        return coordinates;
    }
}
```

---

## Añadir a .gitignore

```gitignore
# Configuración local con API Keys
appsettings.Development.json
appsettings.Production.json

# Variables de entorno
.env
.env.local
*.env

# Archivos de usuario
*.user
*.suo
```

---

## Solución de Problemas

### Error: "Invalid API key" (403)

- **Causa**: API Key incorrecta o no confirmaste tu email
- **Solución**: 
  1. Verifica que copiaste la API Key correctamente
  2. Confirma tu email en OpenCage
  3. Revisa que no haya espacios extra en la configuración

### Error: "Quota exceeded" (402)

- **Causa**: Excediste las 2,500 peticiones/día
- **Solución**: 
  1. Espera hasta medianoche UTC para reset
  2. Implementa caché para reducir peticiones
  3. Considera actualizar a un plan de pago

### Error: "Too many requests" (429)

- **Causa**: Más de 1 petición por segundo
- **Solución**: Implementa rate limiting en tu código

### No se encuentran coordenadas (404)

- **Causa**: El código postal no existe o está mal formateado
- **Solución**: Verifica que el código postal sea válido

---

## Monitoreo

### Dashboard de OpenCage

En [tu dashboard](https://opencagedata.com/dashboard) puedes ver:

- 📊 **Gráficas de uso diario**
- 🔢 **Contador de peticiones**
- ⏰ **Hora del próximo reset**
- 📍 **Últimas peticiones**
- 🌍 **Distribución geográfica**

### Logs en tu aplicación

```csharp
_logger.LogInformation("Geocoding request for {PostalCode}. Remaining quota: {Remaining}/{Limit}", 
    postalCode, 
    response.Rate?.Remaining, 
    response.Rate?.Limit);
```

---

## Ventajas vs Alternativas

### OpenCage vs Google Maps

| Característica | OpenCage | Google Maps |
|----------------|----------|-------------|
| Cuota gratuita | 2,500/día | 40,000/mes (~1,333/día) |
| Requiere tarjeta | ❌ No | ✅ Sí |
| Facilidad de uso | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ |
| Precisión | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| Cobertura global | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| Costo después | $50/mes | $5/1,000 |

### OpenCage vs Nominatim

| Característica | OpenCage | Nominatim |
|----------------|----------|-----------|
| Cuota gratuita | 2,500/día | Ilimitado |
| Rate limit | 1/seg | 1/seg |
| API Key | ✅ Sí | ❌ No |
| Calidad de datos | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ |
| Soporte | ✅ Sí | ❌ Comunidad |
| SLA | ✅ 99.9% | ❌ No |

---

## Recursos

- 📚 [Documentación OpenCage API](https://opencagedata.com/api)
- 🎯 [Dashboard](https://opencagedata.com/dashboard)
- 💬 [Soporte](https://opencagedata.com/contact)
- 📖 [Guías de inicio rápido](https://opencagedata.com/quickstart)
- 🔧 [Ejemplos de código](https://opencagedata.com/code)

---

**Última actualización**: 2025-12-23

