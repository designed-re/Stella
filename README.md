# Stella

Stella is an **unofficial implementation of Konami's e-amusement server**: a .NET 10 ASP.NET Core app with a plugin-based architecture and a built-in admin WebUI.

## Features

- e-amusement wire protocol (KBinXML, LZ77, RC4) with `/eamuse` + `/core` POST routes
- Plugin architecture — drop a plugin DLL + `plugin_<name>.json` into `plugins/` and it loads at runtime
- Built-in **admin WebUI** at `/webui` (server-rendered Razor, dark theme + pastel-pink accent, Tailwind), extensible per-plugin
- SOUND VOLTEX support: EXCEED GEAR (sv6) + NABLA (sv7) unified handlers, plus GRAVITY WARS (sv3) via `game_3.*` routes
- Automatic v6→v7 (EG→∇) profile migration
- **JSON-only configuration** — no environment variables (self-hosted and Docker friendly)
- Docker Compose one-command deployment

## Plugins

Plugins load at runtime from `plugins/`. Each implements `IStellaPlugin` (in `Stella.Abstractions`) and registers handlers with `[StellaHandler(service, module, typeof(Request))]`. Game plugins implement `IStellaGamePlugin` to contribute profile tabs to the WebUI. See `AGENTS.md` for the full plugin/handler contract and the asphyxia parity rules.

### Core Plugin
Built-in handlers shared across all games.

| Service | Method | Description |
|---|---|---|
| `cardmng` | `inquire` | Check if card is registered |
| `cardmng` | `getrefid` | Get/create refid for card |
| `cardmng` | `authpass` | Authenticate card password |
| `cardmng` | `bindmodel` | Bind profile to game code |
| `eacoin` | `checkin` / `consume` / `checkout` | PASELI session |
| `facility` | `get` | Facility/cabinet info |
| `services` | `get` | Server URL list (core services + every registered plugin service prefix, discovered dynamically) |
| `message` | `get` | Server messages |
| `package` | `list` | Package list |
| `pcbevent` | `put` / `pcbtracker` `alive` | PCB keepalive |
| `eventlog` | `write` | Event log |
| `tax` | `get_phase` / `dlstatus` `progress` / `posevent` `income.sales.sale` / `ins` `netlog` | Stubs |

### KFC Plugin (SOUND VOLTEX)
Unified sv6 (EXCEED GEAR) + sv7 (NABLA) handlers, plus sv3 (GRAVITY WARS) via separate `Sv3*Handler` classes registered under `game_3.*`. Each sv6/sv7 handler branches on the game version derived from the e-amusement `model` string.

| Service | Method | Description |
|---|---|---|
| `game` | `sv6_common` / `sv7_common` | Events, courses, valgene, apigene, arena, extend, music_limited |
| `game` | `sv6_new` / `sv7_new` | Create profile (v7 triggers ViiMigrate if v6 exists) |
| `game` | `sv6_load` / `sv7_load` | Load profile (items, params, courses, arena, variant_gate, …) |
| `game` | `sv6_load_m` / `sv7_load_m` | Load scores (v6=21 params, v7=26 params with volforce) |
| `game` | `sv6_load_r` / `sv7_load_r` | Load rival data |
| `game` | `sv6_save` / `sv7_save` | Save profile + items + params + skill + arena + variant_gate |
| `game` | `sv6_save_m` / `sv7_save_m` | Save scores (multi-track, clear-lamp remap for v6) |
| `game` | `sv6_save_c` / `sv7_save_c` | Save course record |
| `game` | `sv6_save_e` / `sv7_save_e` | Save extra (weekly music) |
| `game` | `sv6_save_pb` / `sv7_save_pb` | Save policy break |
| `game` | `sv6_save_valgene` / `sv7_save_valgene` | Save valgene (gacha) items |
| `game` | `sv6_buy` / `sv7_buy` | Purchase items with gamecoin |
| `game` | `sv6_print` / `sv7_print` | Print genesis cards |
| `game` | `sv6_hiscore` / `sv7_hiscore` | All-time high scores |
| `game` | `sv6_lounge` / `sv7_lounge` | Online matchmaking lounge |
| `game` | `sv6_shop` / `sv7_shop` | Shop (returns nxt_time) |
| `game` | `sv6_entry_s` / `sv7_entry_s` | Online matchmaking (in-memory room system) |
| `game` | `sv6_save_mega` / `sv7_save_mega`, `sv6_frozen` / `sv7_frozen`, `sv6_play_e` / `sv7_play_e`, `sv6_play_s` / `sv7_play_s`, `sv6_entry_e` / `sv7_entry_e`, `sv6_exception` / `sv7_exception`, `sv6_log` / `sv7_log` | Stubs |
| `game_3` | `new` / `load` / `save` / `save_m` / `save_c` / `save_pb` / `common` / `load_r` | GRAVITY WARS (sv3) |

The total handler count grows as plugins are added; it is logged at startup (`Total handlers cached:`). `services.get` is built dynamically from the registered handlers, so new plugins are advertised automatically.

## Requirements

- .NET SDK 10
- MariaDB 10.5+ / MySQL 8+
- `KBinXml.Net` git submodule (commit 73d1b9a) — do NOT replace with the NuGet package (the NuGet 2.1.3 lacks the ASCII `0x20` encoding some requests need)
- Node.js only if you want to regenerate the WebUI Tailwind CSS (`Stella.WebUI.Assets/`, `npm run build`) — the built CSS is committed, so this is optional

## Quick Start (Docker)

```bash
git clone --recurse-submodules <repo-url>
cd stella
docker compose up --build
```

The server listens on `http://localhost:8080`. MariaDB starts alongside with `stella_kfc` and `stella_core` databases pre-created (`docker/init-db.sql`). EF migrations run automatically on first boot.

### First-time setup (WebUI)

1. Open `http://localhost:8080/webui` and sign in with the password `stella` (set in `docker/appsettings.json`).
2. Go to **StellaKFCPlugin → Data** and click **Seed static data** (loads `sv_static_*` from `asphyxia_data.json`).
3. On the same page, **Upload** your `music_db.xml` (shift_jis, ~8.4MB) to populate `sv_music`, or use **Reload from disk** if one was baked into the image.

Static-data seeding and `music_db.xml` loading are **not** done at startup anymore — they are on-demand from the WebUI so you control when they happen.

### Configuration (JSON-only — no env vars)

Edit the mounted files on the host and restart the container (`docker compose restart stella`):

| File | Purpose |
|---|---|
| `docker/appsettings.json` | `Urls`, `Stella.ServerUrl` / `ServerHost` / `KeepaliveUrl`, `WebUI.Enabled` / `WebUI.Password` |
| `docker/plugin_core.json` | Core plugin: `db` connection string, `maintenance`, `register_mode`, `private_mode` |
| `docker/plugin_kfc.json` | KFC plugin: `db` connection string + toggles (`unlock_all_songs`, `arena_open`, `arena_session`, `use_blasterpass`, …) |

`Stella.ServerUrl` should be the URL the cabinet can reach the server at (e.g. `http://10.0.1.133:8080/eamuse`); it is returned by `services.get`. The WebUI and e-amusement routes share the same port.

### Volumes

- `stella-db` — MariaDB data
- `stella-seed` — mounted at `/app/Data/Seed`; persists `asphyxia_data.json` and uploaded `music_db.xml` across container recreation (Docker copies the image's seed assets into the volume on first boot)

## Self-hosted (without Docker)

### 1. Prerequisites

- .NET SDK 10 (`dotnet --version` reports 10.x)
- MariaDB or MySQL running and accessible
- `dotnet-ef` (only for generating new migrations):
  ```bash
  dotnet tool install --global dotnet-ef --version 10.0.2
  ```

### 2. Clone

```bash
git clone --recurse-submodules <repo-url>
cd stella
# if you forgot --recurse-submodules:
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

### 4. Configure

Self-hosted defaults are already committed in `CorePlugin/plugin_core.json` and `StellaKFCPlugin/plugin_kfc.json` (`Server=127.0.0.1`). Edit them in place — there are **no environment variables**. Each plugin reads `plugins/plugin_<name>.json` (resolved from the working dir, then the app base directory). Host-level options live in `Stella/appsettings.json` (`Urls`, `Stella.*`, `WebUI.*`).

`plugin_kfc.json`:
```json
{
  "enabled": true,
  "db": "Server=127.0.0.1;Port=3306;User ID=stella;Password=stella;Database=stella_kfc",
  "unlock_all_songs": true,
  "arena_open": true,
  "arena_no_endtime": true,
  "arena_session": 22,
  "arena_station": "None",
  "use_blasterpass": true
}
```

Set `Stella.ServerUrl` in `Stella/appsettings.json` to the URL the cabinet will reach (e.g. `http://YOUR_SERVER_IP:80/eamuse`).

### 5. Build

```bash
dotnet build Stella.slnx
```

Each plugin's PostBuild target copies its DLL + `plugin_*.json` into `Stella/bin/Debug/net10.0/plugins/`.

### 6. Run

```bash
# Development (uses appsettings.Development.json -> http://localhost:8080)
dotnet run --project Stella
```

On startup the server applies EF migrations (`Database.Migrate()`). Then open `http://localhost:8080/webui`, sign in (`stella`), and seed static data / upload `music_db.xml` from the **Data** page.

### 7. Verify

```bash
curl -s http://localhost:8080/webui/login -o /dev/null -w "%{http_code}\n"  # 200
# A GET to /eamuse or / returns 404 — those are POST-only e-amusement routes.
```

## WebUI

The admin WebUI at `/webui` is server-rendered Razor (dark theme, Tailwind, pastel-pink/magenta accent). It is designed to be extended by plugins, not just the host.

- **Auth**: cookie auth, password in `WebUI.Password` (`appsettings.json`). 12h sliding session.
- **Host pages**: dashboard, profiles, login/logout.
- **Plugin pages**: each plugin can contribute top-level pages and profile tabs via `IStellaPlugin.WebUIPages` / `ProfilePages` + `RenderWebUIPageAsync` / `RenderProfileTabAsync`. The KFC plugin contributes Data, Songs List, Startup Flags, Unlock Events, Weekly Score Attack, and profile tabs (Detail, Score, Skill, Achievements, Rivals, Customization, Valkyrie/Premium generators).
- **AJAX**: `POST /webui/api/emit/<pluginId>/<event>` (auth + antiforgery protected). Views call it via `Stella.emit(...)`.

Plugin views are embedded Razor, runtime-compiled. They MUST use `@model object` + `dynamic` (not `@model <PluginType>`) — see `AGENTS.md` for the why and for the full "adding a WebUI page" guide.

## KFC Plugin — EXCEED GEAR + NABLA (+ GRAVITY WARS)

### Static Data

Static game data (events, courses, valgene, apigene, arena, extend, information, music_limited, …) is stored in EF tables (`sv_static_*`) and seeded from `Data/Seed/asphyxia_data.json` (an extract of the asphyxia plugin's `data/*.ts`). Seeding is idempotent and triggered from the WebUI **Data** page.

`music_db.xml` (shift_jis, ~8.4MB) is NOT committed. Upload it from the WebUI **Data** page (or place it in `StellaKFCPlugin/Data/Seed/music_db.xml` and use **Reload from disk**) to populate `sv_music`.

### v6 → v7 Migration

When a v6 (EXCEED GEAR) profile exists and v7 (NABLA) `new` is called, the plugin automatically migrates the profile: copies profile/items/params/scores, remaps clear lamps, recomputes volforce, and resets EX scores for charts Konami reset between versions.

## Migrations

```bash
# Add a migration
dotnet ef migrations add <Name> --project StellaKFCPlugin --startup-project Stella.MigrationHelper

# Apply to database
dotnet ef database update --project StellaKFCPlugin --startup-project Stella.MigrationHelper

# Idempotent SQL script (for manual deployment)
dotnet ef migrations script --idempotent --project StellaKFCPlugin --startup-project Stella.MigrationHelper
```

Or just run the server — `OnAppInitialize` calls `Database.Migrate()` automatically.

## Credits

Originally coded with [KBinXml.Net by Milkitic](https://github.com/Milkitic/KBinXml.Net). Stella adds typed serialization and the e-amusement server implementation.
