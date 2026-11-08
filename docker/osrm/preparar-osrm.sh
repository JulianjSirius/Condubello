#!/usr/bin/env bash
# Descarga el mapa de Colombia (OpenStreetMap, Geofabrik) y lo prepara para OSRM (perfil de auto).
# Es un paso único: tarda unos minutos y necesita ~4 GB de RAM libres. Se puede repetir: reanuda la descarga.
set -euo pipefail
RAIZ="$(cd "$(dirname "$0")/../.." && pwd)"
DATOS="$RAIZ/docker/osrm-data"
IMAGEN="osrm/osrm-backend:v5.25.0"
URL="https://download.geofabrik.de/south-america/colombia-latest.osm.pbf"
PBF="$DATOS/colombia-latest.osm.pbf"
mkdir -p "$DATOS"

# Si otra descarga está en curso, esperarla.
while pgrep -f "curl .*colombia-latest.osm.pbf" >/dev/null; do sleep 10; done

TOTAL="$(curl -sIL "$URL" | tr -d '\r' | awk 'tolower($1)=="content-length:"{n=$2} END{print n}')"
ACTUAL="$(stat -c %s "$PBF" 2>/dev/null || echo 0)"
if [ "$ACTUAL" -lt "$TOTAL" ]; then
  curl -L --fail -C - -o "$PBF" "$URL"
fi

docker run --rm -t -v "$DATOS:/data" "$IMAGEN" osrm-extract   -p /opt/car.lua /data/colombia-latest.osm.pbf
docker run --rm -t -v "$DATOS:/data" "$IMAGEN" osrm-partition  /data/colombia-latest.osrm
docker run --rm -t -v "$DATOS:/data" "$IMAGEN" osrm-customize  /data/colombia-latest.osrm
echo "Listo. Levanta OSRM con: docker compose --profile routing up -d osrm"
