# Stella

Stella is an **unofficial implementation of Konami's e-amusement server**.

## Features
- Partial implementation of e-amusement protocol
- Data exchange between client and server
- Modular and extensible architecture
- Plugin support
- SOUND VOLTEX EXCEED GEAR (sv6) + NABLA (sv7) unified handler support

## Currently Enabled Plugins
- **Core Plugin** — card, facility, eacoin, pcb, services, eventlog
- **KFC Plugin** — SOUND VOLTEX EXCEED GEAR + NABLA (common, load, save, new, hiscore, lounge, play, frozen, buy, print, save_valgene, save_pb, save_e, save_c, save_mega)

## Requirements
- .NET SDK 10
- Supported OS: Windows, Linux
- MariaDB
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

## Manual Build & Run

```bash
# Clone with submodules (KBinXml.Net)
git clone --recurse-submodules <repo-url>
cd stella

# Build
dotnet build Stella.slnx

# Run (default :80, override with ASPNETCORE_URLS)
ASPNETCORE_URLS=http://+:8080 dotnet run --project Stella
```

## KFC Plugin — EXCEED GEAR + NABLA

The KFC plugin serves both sv6 (EXCEED GEAR) and sv7 (NABLA) from unified handler classes. Each handler method branches on the game version derived from the e-amusement `model` string.

### Static Data

Static game data (events, courses, valgene, apigene, arena, extend, information, music_limited, ...) is stored in EF tables (`sv_static_*`) and seeded from `Data/Seed/asphyxia_data.json` — an extract of the asphyxia plugin's `data/exg.ts`, `data/nbl.ts`, `data/ii.ts`, `data/booth.ts`. Seeding is idempotent.

### v6 → v7 Migration

When a v6 (EXCEED GEAR) profile exists and v7 (NABLA) `new` is called, the plugin automatically migrates the profile: copies profile/items/params/scores, remaps clear lamps, recomputes volforce, and resets EX scores for charts Konami reset between versions.

## Migrations

```bash
# Install dotnet-ef if not already
dotnet tool install --global dotnet-ef --version 10.0.2

# Add a migration
dotnet ef migrations add <Name> --project StellaKFCPlugin --startup-project Stella.MigrationHelper

# Apply to database
dotnet ef database update --project StellaKFCPlugin --startup-project Stella.MigrationHelper
```

Or just run the server — `OnAppInitialize` calls `Database.Migrate()` automatically.

## Credits

Originally coded by [KBinXml.Net By Milkitic](https://github.com/Milkitic/KBinXml.Net)
Stella coded to add types when serialization