# Stella

Stella is an **unofficial implementation of Konami's e-amusement server**.

## Features
- Partial implementation of e-amusement protocol
- Data exchange between client and server
- Modular and extensible architecture
- Plugin support
- SOUND VOLTEX EXCEED GEAR (sv6) + NABLA (sv7) unified handler support
- Automatic v6→v7 profile migration
- Docker Compose one-command deployment

## Plugins

### Core Plugin
Built-in handlers shared across all games.

| Service | Method | Description |
|---|---|---|
| `cardmng` | `inquire` | Check if card is registered |
| `cardmng` | `getrefid` | Get/create refid for card |
| `cardmng` | `authpass` | Authenticate card password |
| `cardmng` | `bindmodel` | Bind profile to game code |
| `eacoin` | `checkin` | PASELI session start |
| `eacoin` | `consume` | PASELI payment |
| `eacoin` | `checkout` | PASELI session end |
| `facility` | `get` | Facility/cabinet info |
| `services` | `get` | Server URL list (keepalive, all endpoints) |
| `message` | `get` | Server messages |
| `package` | `list` | Package list |
| `pcbevent` | `put` | PCB event |
| `pcbtracker` | `alive` | PCB keepalive |
| `eventlog` | `write` | Event log |
| `tax` | `get_phase` | Tax phase (stub: phase=0) |
| `dlstatus` | `progress` | Download status (stub) |
| `posevent` | `income.sales.sale` | POS event (stub) |
| `ins` | `netlog` | Net log (stub) |

### KFC Plugin (SOUND VOLTEX)
All handlers serve both sv6 (EXCEED GEAR) and sv7 (NABLA) from unified classes.

| Service | Method | Description |
|---|---|---|
| `game` | `sv6_common` / `sv7_common` | Events, courses, valgene, apigene, arena, extend, music_limited |
| `game` | `sv6_new` / `sv7_new` | Create profile (v7 triggers ViiMigrate if v6 exists) |
| `game` | `sv6_load` / `sv7_load` | Load profile (items, params, courses, arena, variant_gate, etc.) |
| `game` | `sv6_load_m` / `sv7_load_m` | Load scores (v6=21 params, v7=26 params with volforce) |
| `game` | `sv6_load_r` / `sv7_load_r` | Load rival data |
| `game` | `sv6_save` / `sv7_save` | Save profile + items + params + skill + arena + variant_gate |
| `game` | `sv6_save_m` / `sv7_save_m` | Save scores (multi-track, clear-lamp remap for v6) |
| `game` | `sv6_save_c` / `sv7_save_c` | Save course record |
| `game` | `sv6_save_e` / `sv7_save_e` | Save extra (weekly music) |
| `game` | `sv6_save_pb` / `sv7_save_pb` | Save policy break |
| `game` | `sv6_save_valgene` / `sv7_save_valgene` | Save valgene (gacha) items |
| `game` | `sv6_save_mega` / `sv7_save_mega` | Stub |
| `game` | `sv6_frozen` / `sv7_frozen` | Stub |
| `game` | `sv6_buy` / `sv7_buy` | Purchase items with gamecoin |
| `game` | `sv6_print` / `sv7_print` | Print genesis cards |
| `game` | `sv6_hiscore` / `sv7_hiscore` | All-time high scores |
| `game` | `sv6_lounge` / `sv7_lounge` | Online matchmaking lounge |
| `game` | `sv6_shop` / `sv7_shop` | Shop (returns nxt_time) |
| `game` | `sv6_play_e` / `sv7_play_e` | Stub |
| `game` | `sv6_play_s` / `sv7_play_s` | Stub |
| `game` | `sv6_entry_s` / `sv7_entry_s` | Online matchmaking (in-memory room system) |
| `game` | `sv6_entry_e` / `sv7_entry_e` | Entry end (stub) |
| `game` | `sv6_exception` / `sv7_exception` | Stub |
| `game` | `sv6_log` / `sv7_log` | Stub |

**Total: 66 handlers** (18 core + 48 KFC)

## Requirements
- .NET SDK 10
- Supported OS: Windows, Linux
- MariaDB 10.5+ / MySQL 8+
- `KBinXml.Net` git submodule (commit 73d1b9a) — do NOT replace with NuGet package

## Quick Start (Docker)

```bash
git clone --recurse-submodules <repo-url>
cd stella
docker compose up --build
```

The server listens on `http://localhost:8080`. MariaDB starts alongside with `stella_kfc` and `stella_core` databases pre-created. EF migrations and static-data seeding run automatically on first boot.

### Configuration (env vars)

| Env var | Default | Description |
|---|---|---|
| `STELLA_KFC_DB` | `Server=db;...Database=stella_kfc` | KFC plugin DB connection |
| `STELLA_CORE_DB` | `Server=db;...Database=stella_core` | Core plugin DB connection |
| `STELLA_SERVER_URL` | `http://10.0.1.133:8080/eamuse` | Base URL returned by `services.get` |
| `STELLA_SERVER_HOST` | `10.0.1.133` | Keepalive host |
| `STELLA_KFC_UNLOCK_ALL_SONGS` | `true` | Unlock all songs |
| `STELLA_KFC_ARENA_OPEN` | `true` | Keep arena open |
| `STELLA_KFC_ARENA_NO_ENDTIME` | `true` | Arena no end time |
| `STELLA_KFC_ARENA_SESSION` | `22` | Arena session set |
| `STELLA_KFC_ARENA_STATION` | `None` | Arena station set |
| `STELLA_KFC_USE_BLASTERPASS` | `true` | Use BLASTER PASS |
| `STELLA_KFC_UNLOCK_ALL_NAVIGATORS` | `false` | Unlock navigators |
| `STELLA_KFC_UNLOCK_ALL_APPEAL_CARDS` | `false` | Unlock appeal cards |
| `STELLA_KFC_UNLOCK_ALL_VALK_ITEMS` | `false` | Unlock customization items |

### music_db.xml

The KFC plugin loads `Data/Seed/music_db.xml` (shift_jis, ~8.4MB) into the `sv_music` table at startup. This file is NOT committed (too large). Provide it via docker-compose volume mount:

```yaml
volumes:
  - ./StellaKFCPlugin/Data/Seed/music_db.xml:/app/Data/Seed/music_db.xml:ro
```

Or place it in `StellaKFCPlugin/Data/Seed/music_db.xml` locally.

## Manual Build & Run (without Docker)

### 1. Prerequisites

- .NET SDK 10 (`dotnet --version` should report 10.x)
- MariaDB or MySQL running and accessible
- `dotnet-ef` tool installed:
  ```bash
  dotnet tool install --global dotnet-ef --version 10.0.2
  ```

### 2. Clone

```bash
git clone --recurse-submodules <repo-url>
cd stella
```

If you forgot `--recurse-submodules`:
```bash
git submodule update --init --recursive
```

### 3. Create databases

```sql
CREATE DATABASE stella_kfc;
CREATE DATABASE stella_core;
CREATE USER 'stella'@'%' IDENTIFIED BY 'stella';
GRANT ALL PRIVILEGES ON stella_kfc.* TO 'stella'@'%';
GRANT ALL PRIVILEGES ON stella_core.* TO 'stella'@'%';
FLUSH PRIVILEGES;
```

### 4. Configure plugin configs

Copy the example configs and edit the DB connection strings:

```bash
cp CorePlugin/plugin_core.example.json Stella/bin/Debug/net10.0/plugins/plugin_core.json
cp StellaKFCPlugin/plugin_kfc.example.json Stella/bin/Debug/net10.0/plugins/plugin_kfc.json
```

Edit `plugin_core.json`:
```json
{
  "enabled": true,
  "db": "server=localhost;port=3306;database=stella_core;user id=stella;password=stella",
  "maintenance": false
}
```

Edit `plugin_kfc.json`:
```json
{
  "enabled": true,
  "db": "server=localhost;port=3306;database=stella_kfc;user id=stella;password=stella",
  "unlock_all_songs": true,
  "arena_open": true,
  "arena_no_endtime": true,
  "arena_session": 22,
  "arena_station": "None",
  "use_blasterpass": true
}
```

Alternatively, use environment variables instead of JSON files:
```bash
export STELLA_KFC_DB="server=localhost;port=3306;database=stella_kfc;user id=stella;password=stella"
export STELLA_CORE_DB="server=localhost;port=3306;database=stella_core;user id=stella;password=stella"
export STELLA_SERVER_URL="http://YOUR_SERVER_IP:8080/eamuse"
export STELLA_SERVER_HOST="YOUR_SERVER_IP"
```

### 5. Place music_db.xml

```bash
# Place music_db.xml (shift_jis, ~8.4MB) in the seed directory
cp /path/to/music_db.xml StellaKFCPlugin/Data/Seed/music_db.xml
```

### 6. Build

```bash
dotnet build Stella.slnx
```

### 7. Apply migrations (optional — server does this automatically)

```bash
dotnet ef database update --project StellaKFCPlugin --startup-project Stella.MigrationHelper
```

### 8. Run

```bash
# Default port 80
dotnet run --project Stella

# Or specify a port
ASPNETCORE_URLS=http://+:8080 dotnet run --project Stella
```

The server will:
1. Apply EF migrations (`Database.Migrate()`)
2. Seed `sv_static_*` tables from `Data/Seed/asphyxia_data.json`
3. Load `music_db.xml` into `sv_music`

### 9. Verify

```bash
curl http://localhost:8080/
# HTTP 404 is normal (server only handles POST to /eamuse and /core)
```

## KFC Plugin — EXCEED GEAR + NABLA

The KFC plugin serves both sv6 (EXCEED GEAR) and sv7 (NABLA) from unified handler classes. Each handler method branches on the game version derived from the e-amusement `model` string.

### Static Data

Static game data (events, courses, valgene, apigene, arena, extend, information, music_limited, ...) is stored in EF tables (`sv_static_*`) and seeded from `Data/Seed/asphyxia_data.json` — an extract of the asphyxia plugin's `data/exg.ts`, `data/nbl.ts`, `data/ii.ts`, `data/booth.ts`. Seeding is idempotent.

### v6 → v7 Migration

When a v6 (EXCEED GEAR) profile exists and v7 (NABLA) `new` is called, the plugin automatically migrates the profile: copies profile/items/params/scores, remaps clear lamps, recomputes volforce, and resets EX scores for charts Konami reset between versions.

## Migrations

```bash
# Add a migration
dotnet ef migrations add <Name> --project StellaKFCPlugin --startup-project Stella.MigrationHelper

# Apply to database
dotnet ef database update --project StellaKFCPlugin --startup-project Stella.MigrationHelper

# Generate idempotent SQL script (for manual deployment)
dotnet ef migrations script --idempotent --project StellaKFCPlugin --startup-project Stella.MigrationHelper
```

Or just run the server — `OnAppInitialize` calls `Database.Migrate()` automatically.

## Credits

Originally coded by [KBinXml.Net By Milkitic](https://github.com/Milkitic/KBinXml.Net)
Stella coded to add types when serialization