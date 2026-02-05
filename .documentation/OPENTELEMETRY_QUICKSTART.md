# 🚀 Guía Rápida: OpenTelemetry + Grafana Stack

## Inicio Rápido (5 minutos)

### 1. Iniciar el Stack de Observabilidad

```bash
cd .docker
docker-compose --profile infrastructure-monitoring up -d
```

Esto iniciará:
- ✅ OpenTelemetry Collector (puerto 4317/4318)
- ✅ Grafana (puerto 3000)
- ✅ Tempo (puerto 3200)
- ✅ Loki (puerto 3100)
- ✅ Prometheus (puerto 9091)

### 2. Verificar que todo está corriendo

```bash
docker-compose ps
```

Deberías ver todos los servicios en estado `Up`:
```
NAME                STATUS
otel-collector      Up
grafana             Up
tempo               Up
loki                Up
prometheus          Up
```

### 3. Configurar la API para enviar telemetría

Editar `src/Api.Weather.Host/appsettings.Development.json`:

```json
{
  "OpenTelemetry": {
    "EnableOtlpExporter": true,
    "OtlpEndpoint": "http://localhost:4317"
  }
}
```

### 4. Iniciar la API

```bash
dotnet run --project src/Api.Weather.Host
```

### 5. Generar tráfico

```bash
# Hacer una petición a la API
curl -X POST http://localhost:5000/api/v1/weather \
  -H "Content-Type: application/json" \
  -d '{
    "location": "Madrid",
    "time": "2025-12-27T12:00:00Z"
  }'
```

### 6. Visualizar en Grafana

1. Abrir http://localhost:3000
2. Ir a **Explore** (icono de brújula)
3. Seleccionar **Tempo** en el dropdown
4. Click en **Search**
5. Ver las trazas de `WeatherApi`

---

## 📊 Acceso Rápido a los Servicios

| Servicio | URL | Descripción |
|----------|-----|-------------|
| **Grafana** | http://localhost:3000 | Dashboards y visualización |
| **Tempo UI** | http://localhost:3200 | Backend de trazas |
| **Prometheus** | http://localhost:9091 | Métricas |
| **Loki** | http://localhost:3100 | Logs |
| **OTEL Collector** | http://localhost:4317 | Receptor OTLP |

---

## 🔍 Queries Útiles

### En Grafana Explore → Tempo

**Buscar por servicio:**
```
{service.name="WeatherApi"}
```

**Buscar peticiones POST:**
```
{http.method="POST"}
```

**Buscar errores:**
```
{status.code="error"}
```

### En Grafana Explore → Prometheus

**Rate de peticiones HTTP:**
```promql
rate(http_server_request_duration_seconds_count[5m])
```

**Latencia P99:**
```promql
histogram_quantile(0.99, 
  rate(http_server_request_duration_seconds_bucket[5m])
)
```

---

## 🛠️ Comandos Útiles

### Ver logs del collector
```bash
docker logs -f otel-collector
```

### Ver logs de Tempo
```bash
docker logs -f tempo
```

### Reiniciar todo el stack
```bash
cd .docker
docker-compose --profile infrastructure-monitoring restart
```

### Detener todo
```bash
cd .docker
docker-compose --profile infrastructure-monitoring down
```

### Limpiar volúmenes (reset completo)
```bash
cd .docker
docker-compose --profile infrastructure-monitoring down -v
```

---

## 🐛 Troubleshooting

### La API no envía trazas

1. Verificar que `EnableOtlpExporter=true` en appsettings
2. Verificar que OTEL Collector está corriendo:
   ```bash
   docker ps | grep otel-collector
   ```
3. Ver logs del collector:
   ```bash
   docker logs otel-collector
   ```

### No veo trazas en Grafana

1. Verificar que Tempo está configurado como datasource
2. Hacer varias peticiones a la API para generar trazas
3. Esperar 10-15 segundos (hay un pequeño delay)
4. Usar búsqueda por servicio: `{service.name="WeatherApi"}`

### Prometheus no muestra métricas

1. Verificar que Prometheus está scrapeando:
   ```bash
   curl http://localhost:9091/api/v1/targets
   ```
2. La API debe exponer métricas (verificar configuración)

---

## 📈 Próximos Pasos

1. ✅ **Crear dashboards personalizados** en Grafana
2. ✅ **Configurar alertas** en Prometheus
3. ✅ **Agregar logs estructurados** para Loki
4. ✅ **Crear spans personalizados** en los servicios
5. ✅ **Configurar sampling** para producción

Ver documentación completa: [OPENTELEMETRY_CONFIGURATION.md](OPENTELEMETRY_CONFIGURATION.md)

---

**Última actualización**: 2025-12-27

