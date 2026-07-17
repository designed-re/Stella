#!/bin/sh
# Stella entrypoint: render plugin_*.json from env vars, wait for db, then run
# the Stella server. EF migrations + static-data seeding run inside
# OnAppInitialize (context.Database.Migrate() + KfcSeeder.Seed()).
set -e

PLUGINS_DIR="/app/plugins"
mkdir -p "$PLUGINS_DIR"

# ---------------------------------------------------------------------------
# Render plugin_kfc.json from env vars.
# ---------------------------------------------------------------------------
KFC_DB="${STELLA_KFC_DB:-Server=db;Port=3306;User ID=stella;Password=stella;Database=stella_kfc}"
KFC_UNLOCK_ALL_SONGS="${STELLA_KFC_UNLOCK_ALL_SONGS:-true}"
KFC_ARENA_OPEN="${STELLA_KFC_ARENA_OPEN:-true}"
KFC_ARENA_NO_ENDTIME="${STELLA_KFC_ARENA_NO_ENDTIME:-true}"
KFC_ARENA_SESSION="${STELLA_KFC_ARENA_SESSION:-22}"
KFC_ARENA_STATION="${STELLA_KFC_ARENA_STATION:-None}"
KFC_UNLOCK_ALL_NAVIGATORS="${STELLA_KFC_UNLOCK_ALL_NAVIGATORS:-false}"
KFC_UNLOCK_ALL_APPEAL_CARDS="${STELLA_KFC_UNLOCK_ALL_APPEAL_CARDS:-false}"
KFC_UNLOCK_ALL_VALK_ITEMS="${STELLA_KFC_UNLOCK_ALL_VALK_ITEMS:-false}"
KFC_USE_BLASTERPASS="${STELLA_KFC_USE_BLASTERPASS:-true}"

cat > "$PLUGINS_DIR/plugin_kfc.json" <<EOF
{
  "enabled": true,
  "maintenance": false,
  "db": "${KFC_DB}",
  "unlock_all_songs": ${KFC_UNLOCK_ALL_SONGS},
  "arena_open": ${KFC_ARENA_OPEN},
  "arena_no_endtime": ${KFC_ARENA_NO_ENDTIME},
  "arena_session": ${KFC_ARENA_SESSION},
  "arena_station": "${KFC_ARENA_STATION}",
  "unlock_all_navigators": ${KFC_UNLOCK_ALL_NAVIGATORS},
  "unlock_all_appeal_cards": ${KFC_UNLOCK_ALL_APPEAL_CARDS},
  "unlock_all_valk_items": ${KFC_UNLOCK_ALL_VALK_ITEMS},
  "use_blasterpass": ${KFC_USE_BLASTERPASS}
}
EOF
echo "[entrypoint] wrote $PLUGINS_DIR/plugin_kfc.json"

# ---------------------------------------------------------------------------
# Render plugin_core.json from env vars.
# ---------------------------------------------------------------------------
CORE_DB="${STELLA_CORE_DB:-Server=db;Port=3306;User ID=stella;Password=stella;Database=stella_core}"
CORE_REGISTER_MODE="${STELLA_CORE_REGISTER_MODE:-true}"
CORE_PRIVATE_MODE="${STELLA_CORE_PRIVATE_MODE:-false}"

cat > "$PLUGINS_DIR/plugin_core.json" <<EOF
{
  "enabled": true,
  "db": "${CORE_DB}",
  "maintenance": false,
  "register_mode": ${CORE_REGISTER_MODE},
  "private_mode": ${CORE_PRIVATE_MODE}
}
EOF
echo "[entrypoint] wrote $PLUGINS_DIR/plugin_core.json"

export STELLA_KFC_DB="$KFC_DB"
export STELLA_CORE_DB="$CORE_DB"

# ---------------------------------------------------------------------------
# Wait for the database to accept TCP connections.
#
# docker-compose already waits for the MariaDB healthcheck (service_healthy),
# so this is only a safety net for standalone usage. Uses a fast retry loop
# with a short timeout to avoid hanging.
# ---------------------------------------------------------------------------
DB_HOST=$(echo "$KFC_DB" | sed -n 's/.*Server=\([^;]*\).*/\1/p')
DB_PORT=$(echo "$KFC_DB" | sed -n 's/.*Port=\([^;]*\).*/\1/p')
DB_PORT="${DB_PORT:-3306}"
echo "[entrypoint] waiting for db at ${DB_HOST}:${DB_PORT} ..."

i=0
while [ $i -lt 20 ]; do
  # Use timeout to avoid hanging on unreachable hosts.
  if timeout 1 sh -c "echo > /dev/tcp/${DB_HOST}/${DB_PORT}" 2>/dev/null; then
    echo "[entrypoint] db is reachable."
    break
  fi
  i=$((i + 1))
  # Fast retry: 0.5s intervals, 20 attempts = 10s max (vs old 30s).
  sleep 0.5
done

if [ $i -ge 20 ]; then
  echo "[entrypoint] warning: db not reachable after 10s, starting anyway..."
fi

# ---------------------------------------------------------------------------
# Start the Stella server. OnAppInitialize runs:
#   1. context.Database.Migrate() — applies all EF migrations.
#   2. KfcSeeder.Seed() — idempotent load of sv_static_* from
#      Data/Seed/asphyxia_data.json.
#   3. MigrationHelper.LoadMusicDbAsync() — parses Data/Seed/music_db.xml
#      into sv_music.
# ---------------------------------------------------------------------------
cd /app
echo "[entrypoint] starting Stella server on :8080"
exec dotnet Stella.dll --urls http://+:8080
