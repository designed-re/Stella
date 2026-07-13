# Repository Guidelines

Stella is an unofficial implementation of Konami's e-amusement server: a .NET 10 ASP.NET Core web app with a plugin-based architecture. This guide describes how to contribute.

## Project Structure & Module Organization

The solution (`Stella.slnx`) contains:

- `Stella/` — Host web server. Entry point `Stella/Program.cs`; routes under `eamuse/` and `core/`. Holds `Middleware/` (`EAmuseXrpcInputMiddleware`), `Services/` (`PluginService`), `Util/` (RC4, LZ77, KBinXML, EAmuseResponseWriter), and `Data/` JSON assets.
- `Stella.Abstractions/` — Shared contracts: `IStellaPlugin`, `StellaHandler`, `StellaHandlerAttribute`, request/response interfaces.
- `CorePlugin/` — Built-in plugin for core handlers (card, facility, eacoin, pcb, services, eventlog). Layout: `Handlers/`, `Models/`, `EF/` (context), `Migrations/`.
- `StellaKFCPlugin/` — Game-specific plugin (KFC code) for SOUND VOLTEX EXCEED GEAR (sv6) and NABLA (sv7). Same layout as `CorePlugin`, plus `Data/` (IDataProvider, DbDataProvider, Seed/KfcSeeder), `EF/StaticData/` (18 static-data tables), `Util/KfcVersion.cs`, and `Handlers/MigrationHelper.cs`.
- `Stella.MigrationHelper/` — Helper for generating EF Core migrations. Includes `StellaKFCContextFactory` for design-time `dotnet ef` without a live DB.
- `KBinXml.Net/` — Vendored binary XML serializer (git submodule, commit 73d1b9a). Do NOT replace with the NuGet package — the NuGet 2.1.3 lacks ASCII (0x20) encoding support needed for some e-amusement requests.
- `TestClient/` — Console client for manual validation.

Plugins load at runtime from the `plugins/` directory and read config from JSON files such as `plugin_kfc.json`. Only `plugin_*.example.json` files are committed; the real `plugin_*.json` is rendered from environment variables by `docker/entrypoint.sh` or created manually.

## Build, Test, and Development Commands

```bash
dotnet build Stella.slnx       # Build the full solution
dotnet run --project Stella    # Run server (listens on http://+:80 by default; override with ASPNETCORE_URLS)
dotnet ef migrations add <Name> --project StellaKFCPlugin --startup-project Stella.MigrationHelper
```

To apply migrations and seed static data, just run the server — each plugin's `OnAppInitialize` calls `Database.Migrate()` and the KFC plugin seeds the `sv_static_*` tables from `Data/Seed/asphyxia_data.json` and loads `music_db.xml` into `sv_music`.

### Docker

```bash
docker compose up --build      # MariaDB + Stella server on http://localhost:8080
```

The compose stack:
- Creates `stella_kfc` and `stella_core` databases (`docker/init-db.sql`).
- Renders `plugin_kfc.json` / `plugin_core.json` from env vars (`docker/entrypoint.sh`).
- Applies EF migrations and seeds on first boot (idempotent).

Key env vars (see `docker-compose.yml`):
- `STELLA_KFC_DB` / `STELLA_CORE_DB` — DB connection strings.
- `STELLA_SERVER_URL` — Base URL returned by `services.get` (e.g. `http://10.0.1.133:8080/eamuse`).
- `STELLA_SERVER_HOST` — Host for the keepalive URL.
- `STELLA_KFC_UNLOCK_ALL_SONGS`, `STELLA_KFC_ARENA_OPEN`, etc. — plugin toggles.

Each plugin's `PostBuild` target copies its DLL and config into `Stella/bin/Debug/net10.0/plugins/` — preserve this target when adding plugin projects.

## Coding Style & Naming Conventions

- C# with `Nullable` and `ImplicitUsings` enabled; target `net10.0`. No `.editorconfig` exists; follow existing style: 4-space indentation, `PascalCase` for types/public members, `camelCase` for locals/parameters.
- EF contexts live in `EF/`; entities are singular (`Card.cs`). Request/response models in `Models/` follow `<Verb><Subject><Request|Response>` (e.g. `CardInquireRequest`).
- Handlers belong in `Handlers/`, one class per service area. Unified sv6/sv7 handlers branch on the game version derived from the e-amusement `model` string (see `Util/KfcVersion.cs`).

## Plugin & Handler Guidelines

Extend `StellaHandler` and annotate methods with `[StellaHandler(service, module, typeof(Request))]`:

```csharp
public class MyHandler : StellaHandler
{
    [StellaHandler("myservice", "sv6_mymethod", typeof(MyRequest))]
    public async Task<MyResponse> Handle() => await HandleInternal(6);

    [StellaHandler("myservice", "sv7_mymethod", typeof(MyRequest))]
    public async Task<MyResponse> HandleNabla() => await HandleInternal(7);

    private async Task<MyResponse> HandleInternal(int gameVersion)
    {
        var request = Request as MyRequest;
        if (request is null) return new MyResponse { Status = "1" };
        // ...
    }
}
```

`service`/`module` must match the request's `f` query param (`service.method`). New plugins implement `IStellaPlugin` (`Name`, `Version`, `GameCode`, `OnBuilderInitialize`/`OnAppInitialize`) and ship a `plugin_<name>.example.json`.

### KFC Plugin (EXCEED GEAR + NABLA)

The KFC plugin serves both sv6 and sv7 from unified handler classes. Static game data (events, courses, valgene, apigene, arena, extend, music_limited, ...) is stored in EF tables (`sv_static_*`) and seeded from `Data/Seed/asphyxia_data.json` (an extract of the asphyxia plugin's `data/exg.ts`, `data/nbl.ts`, `data/ii.ts`, `data/booth.ts`). `music_db.xml` (shift_jis, ~8.4MB) is loaded into `sv_music` at startup — it is NOT committed; provide it via docker-compose volume mount or place it in `Data/Seed/`.

The `IDataProvider` abstraction (default: `DbDataProvider`) reads static data from EF tables. A JSON-based provider can be selected via the `STELLA_KFC_DATA_MODE=json` env var (reserved for future use).

`ViiMigrate` (in `Handlers/MigrationHelper.cs`) handles EG→∇ profile migration: copies profile/items/params/scores, remaps clear lamps, recomputes volforce, and resets EX scores for charts Konami reset between versions.

## Database & Configuration

- MariaDB/MySQL via Pomelo. Each plugin registers its own `DbContext` and connection string from its JSON config or the `STELLA_*_DB` env var.
- `StellaKFCContext.OnConfiguring` caches the connection string/server version once (lazy) so `new StellaKFCContext()` does not re-read config on every request.
- `StellaKFCContextFactory` (in `Stella.MigrationHelper`) provides a design-time factory for `dotnet ef` so migrations can be generated without a live DB.
- Don't commit secrets — only `plugin_*.example.json` files are tracked. Override with env vars.

## Testing Guidelines

No automated test suite exists yet. `TestClient/` is a manual harness. When adding tests, prefer xUnit with a `*.Tests` project referencing `Stella.Abstractions`.

## Commit & Pull Request Guidelines

Commit messages are short, lowercase, imperative summaries (e.g. `update readme`, `nabla dummy support`). Follow that style.

- Open merge requests against `main` with a description referencing the affected handler/plugin or game code.
- Verify `dotnet build Stella.slnx` succeeds and your plugin appears in `Stella/bin/Debug/net10.0/plugins/` before review.