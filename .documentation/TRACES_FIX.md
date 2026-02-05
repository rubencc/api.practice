# Solución: Las trazas no llegan a Grafana

## Problema Identificado

El OpenTelemetry Collector no podía conectarse a Tempo usando el protocolo gRPC en el puerto 4317, generando errores:
```
connection error: desc = "transport: Error while dialing: dial tcp 172.18.0.7:4317: connect: connection refused"
```

## Correcciones Aplicadas

### 1. Configuración de Tempo (`/monitoring/tempo/tempo.yaml`)

Se especificaron explícitamente los endpoints para los receptores OTLP:

```yaml
distributor:
  receivers:
    otlp:
      protocols:
        http:
          endpoint: "0.0.0.0:4318"
        grpc:
          endpoint: "0.0.0.0:4317"
```

### 2. Configuración del OpenTelemetry Collector (`/monitoring/otel-collector/otel-collector-config.yaml`)

Se cambió el exportador de Tempo de gRPC a HTTP para mayor compatibilidad:

**Antes:**
```yaml
exporters:
  otlp/tempo:
    endpoint: tempo:4317
    tls:
      insecure: true
```

**Después:**
```yaml
exporters:
  otlphttp/tempo:
    endpoint: http://tempo:4318
    tls:
      insecure: true
```

Y se actualizó el pipeline:
```yaml
service:
  pipelines:
    traces:
      receivers: [otlp]
      processors: [batch, attributes]
      exporters: [otlphttp/tempo]  # Cambiado de otlp/tempo
```

## Pasos para Aplicar la Solución

### 1. Reiniciar los Servicios de Monitoreo

Ejecuta el script de corrección:
```bash
cd .docker
chmod +x fix-monitoring.sh
./fix-monitoring.sh
```

O manualmente:
```bash
cd .docker
docker compose --profile infrastructure-monitoring down
docker compose --profile infrastructure-monitoring up -d
```

### 2. Verificar que los Contenedores están Corriendo

```bash
docker ps --filter "name=tempo" --filter "name=otel-collector" --filter "name=grafana"
```

Deberías ver:
- `tempo` (Up)
- `otel-collector` (Up)
- `grafana` (Up)
- `prometheus` (Up)
- `loki` (Up)

### 3. Verificar Logs del OpenTelemetry Collector

```bash
docker logs otel-collector --tail 50
```

Ya **NO** deberías ver errores de conexión a Tempo.

### 4. Ejecutar la Aplicación .NET

```bash
cd src/Api.Weather.Host
dotnet run
```

### 5. Generar Trazas

Realiza peticiones a tu API:
```bash
# Ejemplo
curl -X POST http://localhost:5000/api/v1/weather/forecast \
  -H "Content-Type: application/json" \
  -d '{"location": "Madrid", "days": 5}'
```

### 6. Verificar Trazas en Grafana

1. Abre Grafana: http://localhost:3000
2. Ve a **Explore** (icono de brújula en el menú lateral)
3. Selecciona **Tempo** como datasource
4. En el Query type, selecciona **Search**
5. En Service Name, busca `Api.Weather.Host`
6. Haz clic en **Run Query**

Deberías ver trazas de tus peticiones.

## Verificación de la Configuración de la Aplicación

La aplicación está configurada correctamente en `appsettings.Development.json`:

```json
"OpenTelemetry": {
  "ServiceVersion": "1.0.0",
  "Environment": "Development",
  "EnableConsoleExporter": true,
  "EnableOtlpExporter": true,
  "OtlpEndpoint": "http://localhost:4318",  // ✅ Correcto - apunta al otel-collector
  "EnableAspNetCoreInstrumentation": true,
  "EnableHttpClientInstrumentation": true,
  "EnableRuntimeInstrumentation": true
}
```

## Flujo de las Trazas

```
Aplicación .NET
    ↓ (HTTP/Protobuf)
    ↓ localhost:4318
    ↓
OpenTelemetry Collector
    ↓ (HTTP/Protobuf)
    ↓ tempo:4318
    ↓
Tempo (almacenamiento de trazas)
    ↑
    ↑ (consultas)
    ↑
Grafana (visualización)
```

## Solución de Problemas Comunes

### Problema: Contenedores no inician

```bash
# Verificar logs
docker logs tempo
docker logs otel-collector

# Verificar configuración
docker compose --profile infrastructure-monitoring config
```

### Problema: No veo trazas en Grafana

1. **Verificar que la aplicación está enviando trazas:**
   - Revisa la consola de tu aplicación, deberías ver trazas si `EnableConsoleExporter: true`

2. **Verificar que otel-collector recibe trazas:**
   ```bash
   docker logs otel-collector
   # Busca líneas que indiquen recepción de trazas
   ```

3. **Verificar que Tempo recibe trazas:**
   ```bash
   docker logs tempo
   # Busca líneas sobre ingesta de trazas
   ```

4. **Verificar el datasource de Tempo en Grafana:**
   - Ve a Configuration > Data Sources > Tempo
   - URL debe ser: `http://tempo:3200`
   - Haz clic en "Save & Test"

### Problema: Error "empty ring" en Grafana

Este error está relacionado con una consulta TraceQL mal formada en Grafana. No afecta la ingesta de trazas. 

Para evitarlo:
- Usa consultas TraceQL simples
- Usa la búsqueda por Service Name en lugar de consultas complejas

## Comandos Útiles

```bash
# Ver logs en tiempo real
docker logs -f otel-collector
docker logs -f tempo

# Reiniciar solo un servicio
docker compose restart tempo
docker compose restart otel-collector

# Verificar conectividad
docker exec otel-collector ping -c 3 tempo
docker exec otel-collector wget -O- http://tempo:3200/status/services

# Limpiar todo y empezar de nuevo
docker compose --profile infrastructure-monitoring down -v
docker compose --profile infrastructure-monitoring up -d
```

## Referencias

- [OpenTelemetry Collector - OTLP Exporter](https://github.com/open-telemetry/opentelemetry-collector/tree/main/exporter/otlpexporter)
- [Grafana Tempo - OTLP Receiver](https://grafana.com/docs/tempo/latest/configuration/#distributor)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/instrumentation/net/)

