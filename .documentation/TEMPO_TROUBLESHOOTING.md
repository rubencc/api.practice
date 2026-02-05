# Solución de Problemas de Tempo y TraceQL

## 🐛 Error: "empty ring" en consultas TraceQL

### Síntoma

Grafana muestra el siguiente error al intentar ejecutar consultas TraceQL:

```
failed to execute TraceQL query: {nestedSetParent<0 && true && resource.service.name != nil} | rate() by(resource.service.name) 
Status: 500 Internal Server Error 
Body: error finding generators in Querier.queryRangeRecent: empty ring
```

### Causa

El error "empty ring" indica que Tempo no tiene configurado correctamente el componente de **metrics generator** y **querier** para procesar consultas de métricas generadas a partir de trazas. Este problema ocurre cuando:

1. No está habilitado el `metrics_generator` en Tempo
2. No está configurado el `querier` con el `query_frontend`
3. Los endpoints OTLP no están correctamente configurados
4. Prometheus no está recibiendo las métricas generadas por Tempo

### Solución

#### 1. Configuración completa de Tempo (`tempo.yaml`)

La configuración debe incluir todos estos componentes:

```yaml
server:
  http_listen_port: 3200
  log_level: info

# Query Frontend: Maneja las consultas TraceQL
query_frontend:
  search:
    duration_slo: 5s
    throughput_bytes_slo: 1.073741824e+09
  trace_by_id:
    duration_slo: 5s

# Metrics Generator: Genera métricas a partir de trazas
metrics_generator:
  registry:
    external_labels:
      source: tempo
      cluster: docker-compose
  storage:
    path: /tmp/tempo/generator/wal
    remote_write:
      - url: http://prometheus:9090/api/v1/write
        send_exemplars: true

# Querier: Ejecuta las consultas
querier:
  frontend_worker:
    frontend_address: tempo:9095

# Distributor: Recibe las trazas vía OTLP
distributor:
  receivers:
    otlp:
      protocols:
        http:
        grpc:

# Configuración de procesadores de métricas
overrides:
  defaults:
    metrics_generator:
      processors: [service-graphs, span-metrics]
      generate_native_histograms: both
```

**Componentes clave:**

- **`query_frontend`**: Proporciona la API para consultas TraceQL y optimiza las búsquedas
- **`metrics_generator`**: Genera métricas RED (Rate, Errors, Duration) y service graphs a partir de las trazas
- **`querier`**: Ejecuta las consultas contra los datos almacenados y generados
- **`remote_write`**: Envía las métricas generadas a Prometheus para consultas de agregación

#### 2. Configuración del Docker Compose

```yaml
tempo:
  container_name: tempo
  image: grafana/tempo:${TEMPO_VERSION}
  profiles: [ "infrastructure-monitoring" ]
  command: [ "-config.file=/etc/tempo.yaml" ]
  restart: ${RESTART_ON_FAILURE}
  volumes:
    - /etc/localtime:/etc/localtime:ro
    - ./monitoring/tempo/tempo.yaml:/etc/tempo.yaml
  tmpfs:
    - /tmp/tempo:mode=1777  # ⚠️ Importante: usar tmpfs para evitar problemas de permisos
  ports:
    - 3200:3200  # HTTP API
    - 9095:9095  # gRPC interno (querier <-> query_frontend)
    - 4317       # OTLP gRPC (interno, usado por otel-collector)
    - 4318       # OTLP HTTP (interno, usado por otel-collector)
  networks:
    - localnet
```

**Puntos importantes:**

- Puerto `9095` es necesario para la comunicación interna del querier con el query_frontend
- Puertos `4317` y `4318` NO deben exponerse externamente para evitar conflictos con el otel-collector
- **tmpfs** se usa para `/tmp/tempo` en lugar de volumen nombrado para evitar problemas de permisos
- **Nota**: Los datos no persisten entre reinicios, pero para desarrollo/testing es suficiente

#### 3. Configuración del OpenTelemetry Collector

```yaml
exporters:
  otlp/tempo:
    endpoint: tempo:4317  # ⚠️ Puerto correcto sin mapeo externo
    tls:
      insecure: true
```

#### 4. Reiniciar los servicios

```bash
cd .docker
./restart-monitoring.sh
```

O manualmente:

```bash
cd .docker
docker compose --profile infrastructure-monitoring down
docker compose --profile infrastructure-monitoring up -d
```

**Nota**: Como Tempo usa tmpfs, no hay datos persistentes que limpiar. El flag `-v` solo es necesario si quieres limpiar también los volúmenes de otros servicios.

### Verificación

#### 1. Verificar que Tempo está recibiendo trazas

```bash
curl http://localhost:3200/status/services
```

Deberías ver todos los servicios como "Running":
- distributor
- ingester
- querier
- query-frontend
- metrics-generator
- compactor

#### 2. Verificar que Prometheus recibe métricas de Tempo

Abre Prometheus en `http://localhost:9091` y busca métricas que empiecen con `traces_`:

```
traces_spanmetrics_latency_bucket
traces_service_graph_request_total
```

#### 3. Verificar logs de Tempo

```bash
docker logs tempo | grep -i "generator\|querier\|frontend"
```

No debería haber errores relacionados con "ring" o "generator".

#### 4. Verificar Loki (tarda ~30 segundos)

```bash
curl http://localhost:3100/ready
```

**Nota**: Loki espera 15 segundos después de estar listo por seguridad. Si ves "Ingester not ready: waiting for 15s after being ready", es normal y estará listo en unos segundos.

#### 5. Probar consulta TraceQL en Grafana

En el explorador de Grafana, usando Tempo como datasource:

```traceql
{resource.service.name="WeatherApi"}
```

Esta consulta básica debería devolver trazas sin error.

## 🔍 Conceptos Clave

### ¿Qué es el "ring"?

En Tempo (basado en Grafana Tempo), un "ring" es una estructura de datos que permite a los diferentes componentes (distributors, ingesters, queriers) conocerse entre sí en un deployment distribuido. En modo single-binary (nuestro caso), el ring debería inicializarse automáticamente.

El error "empty ring" significa que el **querier no puede encontrar instancias de metrics generator** para consultar métricas generadas.

### ¿Por qué necesitamos metrics generator?

TraceQL tiene dos tipos de consultas:

1. **Consultas de trazas**: `{resource.service.name="WeatherApi"}` - busca trazas específicas
2. **Consultas de métricas**: `{} | rate() by(resource.service.name)` - agrega métricas a partir de trazas

El segundo tipo requiere que el **metrics_generator** esté activo y conectado al querier.

## 📚 Referencias

- [Tempo Metrics Generator](https://grafana.com/docs/tempo/latest/metrics-generator/)
- [TraceQL Language](https://grafana.com/docs/tempo/latest/traceql/)
- [Tempo Configuration](https://grafana.com/docs/tempo/latest/configuration/)

