#!/bin/bash

echo "=== Diagnóstico y Corrección de Monitoreo ==="
echo ""

# Verificar si docker compose está disponible
if command -v docker &> /dev/null; then
    echo "✅ Docker está disponible"
else
    echo "❌ Docker no está disponible"
    exit 1
fi

cd "$(dirname "$0")"

echo ""
echo "📍 Directorio actual: $(pwd)"
echo ""

# Detener contenedores
echo "🛑 Deteniendo contenedores de monitoreo..."
docker compose --profile infrastructure-monitoring stop 2>&1

echo ""
echo "🗑️ Eliminando contenedores..."
docker compose --profile infrastructure-monitoring rm -f 2>&1

echo ""
echo "🚀 Iniciando contenedores..."
docker compose --profile infrastructure-monitoring up -d 2>&1

echo ""
echo "⏳ Esperando 10 segundos..."
sleep 10

echo ""
echo "📊 Estado de los contenedores:"
docker ps --filter "name=tempo" --filter "name=otel-collector" --filter "name=grafana" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

echo ""
echo "🔍 Verificando conectividad de otel-collector a Tempo..."
sleep 5
docker logs otel-collector --tail 20 2>&1 | grep -i "tempo\|error\|connection" || echo "Sin errores recientes"

echo ""
echo "✅ Proceso completado"
echo ""
echo "🌐 URLs de acceso:"
echo "  - Grafana: http://localhost:3000"
echo "  - Tempo: http://localhost:3200"
echo "  - Prometheus: http://localhost:9091"
echo "  - Loki: http://localhost:3100"
echo ""
echo "Para verificar las trazas:"
echo "  1. Ejecuta tu aplicación .NET"
echo "  2. Realiza algunas peticiones a la API"
echo "  3. Abre Grafana y ve a Explore > Tempo"
echo "  4. Busca trazas de 'Api.Weather.Host'"

