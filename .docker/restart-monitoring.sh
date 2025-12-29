#!/bin/bash

# Script para reiniciar los servicios de monitoreo con limpieza de volúmenes
# Uso: ./restart-monitoring.sh

set -e

echo "🛑 Deteniendo servicios de monitoreo..."
docker compose --profile infrastructure-monitoring down -v

echo "🧹 Limpiando volúmenes de Tempo (opcional, descomenta si necesitas limpieza completa)..."
# docker volume rm docker_tempo_data 2>/dev/null || true
# docker volume rm docker_otelcollector_log 2>/dev/null || true

echo "🚀 Iniciando servicios de monitoreo..."
docker compose --profile infrastructure-monitoring up -d

echo "⏳ Esperando a que los servicios estén listos..."
sleep 15

echo "✅ Verificando estado de Tempo..."
for i in {1..5}; do
    if curl -f http://localhost:3200/status/services 2>/dev/null; then
        echo "✅ Tempo está respondiendo correctamente"
        break
    else
        if [ $i -eq 5 ]; then
            echo "⚠️ Tempo no responde aún, verifica los logs: docker logs tempo"
        else
            echo "⏳ Esperando a Tempo... (intento $i/5)"
            sleep 3
        fi
    fi
done

echo ""
echo "✅ Verificando estado de Prometheus..."
if curl -f http://localhost:9091/-/ready 2>/dev/null; then
    echo "✅ Prometheus está respondiendo correctamente"
else
    echo "⚠️ Prometheus no responde aún"
fi

echo ""
echo "✅ Verificando estado de Loki..."
for i in {1..5}; do
    if curl -f http://localhost:3100/ready 2>/dev/null; then
        echo "✅ Loki está respondiendo correctamente"
        break
    else
        if [ $i -eq 5 ]; then
            echo "⚠️ Loki no responde aún, verifica los logs: docker logs loki"
        else
            echo "⏳ Esperando a Loki... (intento $i/5)"
            sleep 3
        fi
    fi
done

echo ""
echo "📊 URLs de acceso:"
echo "   Grafana:    http://localhost:3000"
echo "   Prometheus: http://localhost:9091"
echo "   Tempo:      http://localhost:3200"
echo "   Loki:       http://localhost:3100"
echo ""
echo "🔍 Para verificar logs:"
echo "   docker logs tempo"
echo "   docker logs otel-collector"
echo "   docker logs prometheus"
echo "   docker logs grafana"
echo ""
echo "✅ Servicios de monitoreo reiniciados!"

