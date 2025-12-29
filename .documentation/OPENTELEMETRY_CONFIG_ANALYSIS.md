# 🔍 Análisis de Configuración OpenTelemetry

## 📋 Resumen Ejecutivo

Se han identificado y corregido **3 problemas críticos** en la configuración de OpenTelemetry que impedían el correcto envío de trazas y métricas al OpenTelemetry Collector.

---

## ❌ Problemas Encontrados y Corregidos

### 1. **Protocolo OTLP Incorrecto** ⚠️ CRÍTICO

**Problema**:
- El `appsettings.json` configuraba el endpoint HTTP: `http://localhost:4318`
- El código usaba `AddOtlpExporter()` sin especificar protocolo
- Por defecto, OpenTelemetry SDK usa **gRPC** (puerto 4317), no HTTP

**Impacto**:
```
❌ Las trazas y métricas NO se enviaban al collector
❌ Grafana Tempo no recibía datos
❌ Prometheus no recibía métricas
```

**Solución Aplicada**:
```csharp
tracerProviderBuilder.AddOtlpExporter(otlpOptions =>
{
    otlpOptions.Endpoint = new Uri(options.OtlpEndpoint);
    otlpOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf; // ✅ Especificado
});
```

---

### 2. **Binding de Configuración Incompleto** ⚠️ ALTO

**Problema**:
```csharp
// ❌ ANTES: No especificaba la sección
services.AddOptions<OpenTelemetryOptions>()
    .Bind(configuration)  // Buscaba en la raíz
```

Esto causaba que las opciones no se cargaran correctamente desde `appsettings.json`.

**Solución Aplicada**:
```csharp
// ✅ DESPUÉS: Especifica la sección correcta
services.AddOptions<OpenTelemetryOptions>()
    .Bind(configuration.GetSection("OpenTelemetry"))
```

---

### 3. **Configuración Redundante** ⚠️ MENOR

**Problema**:
```csharp
// ❌ ANTES: Se leía dos veces el mismo valor
.Configure(options =>
{
    // ... código ...
    options.OtlpEndpoint = configuration
        .GetValue<string>("OpenTelemetry:OtlpEndpoint"); // Ya está en Bind()
})
```

**Solución Aplicada**:
Eliminado, ya que `.Bind()` ya carga automáticamente todas las propiedades.

---

## ✅ Configuración Final Correcta

### appsettings.Development.json ✅
```json
{
  "OpenTelemetry": {
    "ServiceVersion": "1.0.0",
    "Environment": "Development",
    "EnableConsoleExporter": true,        // ✅ Para debug local
    "EnableOtlpExporter": true,           // ✅ Para enviar a collector
    "OtlpEndpoint": "http://localhost:4318", // ✅ HTTP endpoint
    "EnableAspNetCoreInstrumentation": true,
    "EnableHttpClientInstrumentation": true,
    "EnableRuntimeInstrumentation": true
  }
}
```

**Nota**: El endpoint `http://localhost:4318` es correcto porque ahora especificamos `HttpProtobuf` en el código.

### OpenTelemetry Collector ✅
```yaml
receivers:
  otlp:
    protocols:
      grpc:
        endpoint: "0.0.0.0:4317"  # Para clientes gRPC
      http:
        endpoint: "0.0.0.0:4318"  # ✅ Para nuestra app (HTTP)
        cors:
          allowed_origins:
            - http://*
            - https://*
```

**✅ Correcto**: El collector escucha en ambos protocolos.

### Tempo ✅
```yaml
distributor:
  receivers:
    otlp:
      protocols:
        http:   # ✅ Recibe de collector vía HTTP
        grpc:   # ✅ Recibe de collector vía gRPC
```

**✅ Correcto**: Tempo acepta ambos protocolos del collector.

---

## 🔄 Flujo de Datos Correcto

```
┌─────────────────┐
│  Weather API    │
│  (tu app)       │
│                 │
│  OTLP HTTP      │
│  :4318          │
└────────┬────────┘
         │
         │ HTTP Protobuf
         │ localhost:4318
         ▼
┌─────────────────┐
│ OTEL Collector  │
│                 │
│ Recibe: :4318   │
│ Procesa         │
│ Enruta          │
└────┬──────┬─────┘
     │      │
     │      └──────────┐
     │                 │
     │ OTLP gRPC      │ OTLP HTTP
     │ tempo:4317     │ loki:3100
     ▼                 ▼
┌─────────┐      ┌─────────┐
│  Tempo  │      │  Loki   │
│ (traces)│      │ (logs)  │
└─────────┘      └─────────┘
```

**Prometheus** recibe métricas del collector vía scraping en el puerto 8890.

---

## 🧪 Verificación

### 1. Compilar y Ejecutar la API
```bash
cd src/Api.Weather.Host
dotnet build
dotnet run
```

### 2. Generar una Traza
```bash
curl https://localhost:5001/api/v1/weather/forecast?location=Madrid
```

### 3. Verificar en Collector
```bash
docker logs otel-collector --tail 50
```

**Buscar**:
```
level=info msg="Traces received"
level=info msg="Metrics received"
```

### 4. Verificar en Tempo
```bash
curl http://localhost:3200/api/search?tags=service.name=Api.Weather.Host | jq
```

**Esperado**: Debe devolver trazas con el nombre del servicio.

### 5. Verificar en Grafana
1. Abrir http://localhost:3000
2. Ir a **Explore**
3. Seleccionar **Tempo** como datasource
4. Ejecutar consulta TraceQL:
   ```traceql
   {resource.service.name="Api.Weather.Host"}
   ```

**Esperado**: Ver trazas de la aplicación con todos los spans.

---

## 📊 Configuración de Protocolos OTLP

### Opciones Disponibles

| Protocolo | Puerto | Uso | Configuración |
|-----------|--------|-----|---------------|
| **gRPC** | 4317 | Más eficiente, binario | `OtlpExportProtocol.Grpc` |
| **HTTP Protobuf** | 4318 | Compatible con firewalls | `OtlpExportProtocol.HttpProtobuf` ✅ |
| **HTTP JSON** | 4318 | Debugging, legible | `OtlpExportProtocol.HttpJson` |

**Recomendación**: Usar **HTTP Protobuf** (actual) porque:
- ✅ Compatible con la mayoría de infraestructuras
- ✅ No bloqueado por firewalls corporativos
- ✅ Buen balance entre eficiencia y compatibilidad
- ✅ Funciona bien con Docker networking

---

## 🔧 Alternativa: Usar gRPC

Si prefieres usar gRPC (más eficiente):

### Cambiar appsettings.json
```json
{
  "OpenTelemetry": {
    "OtlpEndpoint": "http://localhost:4317",  // ⬅️ Cambiar a 4317
    // ... resto igual
  }
}
```

### Cambiar ServiceCollectionExtensions.cs
```csharp
tracerProviderBuilder.AddOtlpExporter(otlpOptions =>
{
    otlpOptions.Endpoint = new Uri(options.OtlpEndpoint);
    otlpOptions.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc; // ⬅️ Cambiar a Grpc
});
```

---

## 📝 Checklist de Verificación

### Antes de Desplegar

- [x] ✅ Protocolo OTLP especificado en código
- [x] ✅ Endpoint correcto en appsettings
- [x] ✅ Binding de configuración correcto
- [x] ✅ Collector configurado para recibir HTTP
- [x] ✅ Tempo configurado para recibir del collector
- [x] ✅ Servicios de monitoreo en ejecución

### Después de Desplegar

- [ ] ⏳ Logs de collector muestran trazas recibidas
- [ ] ⏳ Tempo responde con trazas en API search
- [ ] ⏳ Grafana muestra trazas en Explore
- [ ] ⏳ Prometheus tiene métricas `http_server_duration_*`

---

## 🐛 Troubleshooting

### Problema: No aparecen trazas en Grafana

**1. Verificar que la app está enviando**:
```bash
# Activar logs de debug en appsettings
"Logging": {
  "LogLevel": {
    "OpenTelemetry": "Debug"
  }
}
```

**2. Verificar collector**:
```bash
docker logs otel-collector --tail 100 | grep -i "error\|warn"
```

**3. Verificar Tempo**:
```bash
curl http://localhost:3200/status/services | grep -i "running\|fail"
```

### Problema: Error "connection refused"

**Causa**: El collector no está accesible desde la app.

**Solución**:
```bash
# Verificar que collector está corriendo
docker ps | grep otel-collector

# Verificar puertos
netstat -tulpn | grep 4318
```

### Problema: Trazas en consola pero no en Grafana

**Causa**: `EnableOtlpExporter: false` o protocolo incorrecto.

**Solución**:
```json
{
  "OpenTelemetry": {
    "EnableOtlpExporter": true  // ⬅️ Verificar
  }
}
```

---

## 📚 Referencias

- [OpenTelemetry .NET SDK](https://github.com/open-telemetry/opentelemetry-dotnet)
- [OTLP Specification](https://opentelemetry.io/docs/specs/otlp/)
- [Tempo Documentation](https://grafana.com/docs/tempo/latest/)
- [OTEL Collector Configuration](https://opentelemetry.io/docs/collector/configuration/)

---

## ✅ Resumen de Cambios

| Archivo | Cambio | Razón |
|---------|--------|-------|
| `ServiceCollectionExtensions.cs` | Agregado `Protocol = HttpProtobuf` | Especificar protocolo HTTP |
| `ServiceCollectionExtensions.cs` | Corregido `.Bind(configuration.GetSection("OpenTelemetry"))` | Cargar sección correcta |
| `ServiceCollectionExtensions.cs` | Eliminado lectura redundante de `OtlpEndpoint` | Ya cargado por Bind |

**Estado**: ✅ **CONFIGURACIÓN CORREGIDA Y LISTA PARA USAR**

---

**Fecha**: 2025-12-29  
**Revisado por**: GitHub Copilot  
**Prioridad**: 🔴 CRÍTICA - Desplegar inmediatamente

