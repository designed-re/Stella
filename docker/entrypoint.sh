#!/bin/sh
# Stella entrypoint: wait for the DB to accept TCP connections, then run the
# Stella server. All configuration is JSON-only (appsettings.json +
# plugins/plugin_*.json, mounted by docker-compose); there are no STELLA_*
# environment variables.
#
# At startup each plugin runs Database.Migrate() so the schema is ready.
# Static-data seeding (sv_static_* from asphyxia_data.json) and music_db.xml
# loading into sv_music are NOT done at startup — trigger them from the WebUI
# "Data" page (http://<host>:8080/webui) after first boot.
set -e

# Resolve the DB host/port from the mounted plugin_kfc.json so we can wait for it.
KFC_DB="$(sed -n 's/.*"db"[[:space:]]*:[[:space:]]*"\([^"]*\)".*/\1/p' /app/plugins/plugin_kfc.json | head -1)"
DB_HOST=$(echo "$KFC_DB" | sed -n 's/.*Server=\([^;]*\).*/\1/p')
DB_PORT=$(echo "$KFC_DB" | sed -n 's/.*Port=\([^;]*\).*/\1/p')
DB_PORT="${DB_PORT:-3306}"
echo "[entrypoint] waiting for db at ${DB_HOST}:${DB_PORT} ..."

i=0
while [ $i -lt 40 ]; do
  if timeout 1 sh -c "echo > /dev/tcp/${DB_HOST}/${DB_PORT}" 2>/dev/null; then
    echo "[entrypoint] db is reachable."
    break
  fi
  i=$((i + 1))
  sleep 0.5
done

if [ $i -ge 40 ]; then
  echo "[entrypoint] warning: db not reachable after 20s, starting anyway..."
fi

cd /app
exec dotnet Stella.dll
