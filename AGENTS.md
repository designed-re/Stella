# Repository Guidelines

Stella is an unofficial implementation of Konami's e-amusement server: a .NET 10 ASP.NET Core web app with a plugin-based architecture. This guide describes how to contribute.

## Architecture & Request Lifecycle

A request travels through a fixed pipeline. Read this first to understand how the pieces in the rest of the document fit together.

### The e-amusement wire format

Every game call is an HTTP POST whose **body** is a KBinXML document, possibly LZ77-compressed and RC4-encrypted. Routing is in the **query string**:

- Modern games: `?f=service.method` (e.g. `?f=game.sv6_common`).
- Legacy games (GRAVITY WARS sv3): `?module=services&method=get`.

Identifying headers (checked by `EAmuseXrpcInputMiddleware.IsEAmuseRequest`):
- `User-Agent: EAMUSE.XRPC/1.0` (modern) **or** `EAMUSE.Httpac/1.0` (sv3).
- `X-Compress: lz77` or `none`.
- Optional `X-Eamuse-Info` — the RC4 key material; its presence means the body is encrypted.

### Startup order (`Stella/Program.cs`)

1. `PluginService.LoadPluginsAsync()` — loads every `*.dll` from `plugins/`, instantiates classes implementing `IStellaPlugin`, registers them in `StellaPluginRegistry`.
2. For each plugin: `OnBuilderInitialize(builder)` (registers its `DbContext` + resolves config), then `RegisterPluginConfig` scans the assembly for `[StellaHandler]` methods and caches them in `_handlerCache` keyed `service:module`.
3. `OnAppInitialize(app)` per plugin — CorePlugin and StellaKFCPlugin call `context.Database.Migrate()` so the schema is ready. Static-data seeding (`KfcSeeder.Seed()`, `sv_static_*` from `Data/Seed/asphyxia_data.json`) and `music_db.xml` loading into `sv_music` are **no longer** run at startup — they are triggered on demand from the KFC WebUI "Data" page (see the WebUI section). `ViiMigrateAsync` still runs on first `sv7_new`.
4. `EAmuseXrpcInputMiddleware` is registered, then two minimal API POST groups: `/eamuse` and `/core`. Both run identical routing logic — the path only distinguishes which games point where.

### Inbound processing (middleware)

`EAmuseXrpcInputMiddleware.InvokeAsync` only acts on POSTs whose path contains `eamuse` and which pass `IsEAmuseRequest`. For those:

1. `ReadAndProcessBodyAsync` reads the raw body.
2. `RC4.ApplyEAmuseInfo` decrypts if `X-Eamuse-Info` is present.
3. `LZ77.Decompress` decompresses if `X-Compress: lz77`.
4. `KbinConverter.ReadXmlLinq` parses the bytes into an `XDocument`.
5. The result is stored in `HttpContext.Items["ea"]` as `EAmuseXrpcData { Document, Encoding = SHIFT-JIS, EAmuseInfo }`.

Non-matching requests pass straight through (`await _next`); the route handler sees `Items["ea"] == null` and returns.

### Dispatch (`PluginService.InvokeHandler`)

The route handler resolves `service`/`method` (preferring `f`, falling back to `module`+`method`), then calls `InvokeHandler(service, method, doc, model, ctx)`:

1. Cache lookup: `_handlerCache["{service}:{method}"]` -> `(HandlerType, MethodInfo, PluginConfig, Logger)`.
2. Instantiates the handler (`StellaHandler` subclass) and pre-processes the request `XDocument`:
   - `PreprocessXmlForArrays` — splits space-separated arrays like `<gip __count="7">0 0 0 8 0 0 0</gip>` into seven `<gip>` elements, so `XmlSerializer` can deserialize them.
   - `NormalizeRootElementName` — renames the `<call>` child (e.g. `<game_3>`) to the request model's `[XmlRoot]` name (e.g. `game`), since `XmlSerializer` rejects mismatched roots.
3. Deserializes into the request type from `[StellaHandlerAttribute.RequestType]`, assigns `handler.Request`, `Model`, `PluginConfig`, `Logger`, `PCBId` (from the `srcid` attribute), `HttpContext`.
4. Invokes the method via reflection. Sync results are wrapped in `Task.FromResult`; async `Task<T>` results are unwrapped to `IStellaEAmuseResponse`.

`StellaHandlerException` (carrying an error code) propagates out so the route handler can emit a `<response status="..."/>` error reply via `EAmuseResponseWriter.BuildStatusResponse`.

### Outbound serialization (`Program.WriteEAmuseResponseAsync`)

1. A `<response>` wrapper is opened. For `IStellaMultiElementResponse` (asphyxia `send.object([{...},...])`), each element is serialized as a sibling; otherwise the single response object is serialized.
2. `XmlSerializer` emits elements in **property declaration order** — that order IS the wire order the game expects.
3. `XDocumentTypeExtensions.AddKBinTypesFromResponse` walks the document and adds `__type`/`__count` attributes derived from the C# types (e.g. `int` -> `s32`, `byte` -> `u8`, `List<int>` -> `s32 __count="N"`). It special-cases IP fields (`ip`/`gip`/`lip`/`*_ip` -> `ip4`) and string values that look like IPv4.
4. `KbinConverter.Write` encodes the document (SHIFT-JIS by default).
5. `LZ77.Compress` is attempted; if smaller, `X-Compress: lz77` is used, otherwise `none`.
6. `RC4.ApplyEAmuseInfo` re-encrypts when the original request was encrypted.
7. Response headers (`X-Eamuse-Info`, `X-Compress`, `Content-Type: application/octet-stream`) are set and the bytes are written.

### Plugin model

A plugin is any assembly in `plugins/` containing an `IStellaPlugin` implementation. Contracts in `Stella.Abstractions`:

- `IStellaPlugin` — metadata (`Name`, `Version`, `GameCode`, `MinVer`/`MaxVer`), lifecycle hooks (`OnBuilderInitialize`, `OnAppInitialize`), and `ProfileExistsAsync(refid)` used by `cardmng.inquire` to report the `binded` flag.
- `IStellaPluginConfig` — `db` (connection string) + `enabled` flag; concrete configs add plugin toggles.
- `StellaHandler` — base class exposing `Request`, `Model`, `PCBId`, `PluginConfig`, `Logger`, `HttpContext` to handler methods.
- `[StellaHandler(service, module, typeof(Request))]` — registers a method in the handler cache.
- `StellaPluginRegistry` — process-wide registry letting CorePlugin look up game plugins by `GameCode` without a project reference (mirrors asphyxia's `ROOT_CONTAINER.getPluginByCode`).
- `IStellaMultiElementResponse` — responses that serialize as several sibling elements.

### Two shipped plugins

- **CorePlugin** (`GameCode: STELLA_PROTOCOL`) — cross-game handlers: `cardmng.*` (inquire/authpass/getrefid/bindmodel), `facility.get`, `message.*`, `eacoin.*`, `pcbevent.put`, `pcbtracker.alive`, `services.get`. `cardmng.inquire` uses `StellaPluginRegistry.GetByGameCode` to ask the game plugin whether a profile exists, so the `binded` flag is accurate.
- **StellaKFCPlugin** (`GameCode: KFC`) — SOUND VOLTEX. Unified sv6/sv7 handlers branch on `KfcVersion.GetVersion(Model)`; separate `Sv3*Handler` classes cover GRAVITY WARS. Static data is served through `IDataProvider` (default `DbDataProvider`) reading `sv_static_*` tables seeded by `KfcSeeder`. `ViiMigrateAsync` (in `Handlers/MigrationHelper.cs`) copies an EG profile to NABLA on first `sv7_new`, remapping clear lamps and recomputing volforce from `music_db.xml` difficulty levels.

### Configuration resolution precedence

Configuration is **JSON-only — no environment variables**. Each plugin's DB connection string comes from the `db` field of its `plugin_<name>.json` (resolved by `StellaKFCContext`/`CoreContext.ResolvePluginConfigPath`, which checks `plugins/<name>.json` under the working dir then under `AppContext.BaseDirectory`). Host-level options (`ServerUrl`, `ServerHost`, `KeepaliveUrl`, `WebUI.Enabled`, `WebUI.Password`) come from `appsettings.json` and are bound once at startup by `Stella.Abstractions.Configuration.StellaOptions`. `StellaKFCContext`/`CoreContext` cache the connection string + MariaDB server version once so `new StellaKFCContext()` is cheap per request. `docker/entrypoint.sh` no longer renders config from env — it just waits for the DB and runs the server; `docker-compose.yml` mounts `docker/appsettings.json` and `docker/plugin_*.json` (with `Server=db`) over the app.


## Project Structure & Module Organization

The solution (`Stella.slnx`) contains:

- `Stella/` — Host web server. Entry point `Stella/Program.cs`; routes under `eamuse/` and `core/`. Holds `Middleware/` (`EAmuseXrpcInputMiddleware`), `Services/` (`PluginService`), `Util/` (RC4, LZ77, KBinXML, EAmuseResponseWriter), and `Data/` JSON assets.
- `Stella.Abstractions/` — Shared contracts: `IStellaPlugin`, `StellaHandler`, `StellaHandlerAttribute`, request/response interfaces.
- `CorePlugin/` — Built-in plugin for core handlers (card, facility, eacoin, pcb, services, eventlog). Layout: `Handlers/`, `Models/`, `EF/` (context), `Migrations/`.
- `StellaKFCPlugin/` — Game-specific plugin (KFC code) for SOUND VOLTEX EXCEED GEAR (sv6) and NABLA (sv7). Same layout as `CorePlugin`, plus `Data/` (IDataProvider, DbDataProvider, Seed/KfcSeeder), `EF/StaticData/` (18 static-data tables), `Util/KfcVersion.cs`, and `Handlers/MigrationHelper.cs`.
- `Stella.MigrationHelper/` — Helper for generating EF Core migrations. Includes `StellaKFCContextFactory` for design-time `dotnet ef` without a live DB. It must NOT contain a source `plugins/` folder (the Web SDK would glob it as content, copy it to the output, and re-glob the copy on the next build, producing `bin/Debug/net10.0/bin/Debug/...` infinite nesting). Its `.csproj` excludes `plugins/**` from content and instead links `plugin_kfc.json` into the output `plugins/` dir as a single file.
- `KBinXml.Net/` — Vendored binary XML serializer (git submodule, commit 73d1b9a). Do NOT replace with the NuGet package — the NuGet 2.1.3 lacks ASCII (0x20) encoding support needed for some e-amusement requests.
- `TestClient/` — Console client for manual validation.

Plugins load at runtime from the `plugins/` directory and read config from JSON files such as `plugin_kfc.json`. Both `plugin_*.example.json` (template) **and** `plugin_*.json` (self-hosted defaults, `Server=127.0.0.1`) are committed. For Docker, `docker/plugin_*.json` (with `Server=db`) are mounted over the app by `docker-compose.yml`. There are **no environment variables** anywhere — edit the JSON files directly.

## Build, Test, and Development Commands

```bash
dotnet build Stella.slnx       # Build the full solution
dotnet run --project Stella    # Run server (listens on http://+:80 by default; override with ASPNETCORE_URLS)
dotnet ef migrations add <Name> --project StellaKFCPlugin --startup-project Stella.MigrationHelper
```

To apply migrations, just run the server — each plugin's `OnAppInitialize` calls `Database.Migrate()`. Static-data seeding (`sv_static_*` from `Data/Seed/asphyxia_data.json`) and `music_db.xml` loading into `sv_music` are **not** run at startup anymore — use the KFC WebUI "Data" page (see the WebUI section) to seed static data and upload `music_db.xml` on demand.

### Self-hosted (no Docker)

```bash
dotnet build Stella.slnx
# Edit Stella/appsettings.json (Urls, Stella.ServerUrl/ServerHost, WebUI.Password)
# and plugins/plugin_kfc.json (db connection string, toggles) as needed.
dotnet run --project Stella
```

A MariaDB/MySQL instance must be reachable at the `db` connection string in each `plugin_*.json`. The server reads `appsettings.json` (`Urls`, `Stella`, `WebUI`) and each plugin's `plugin_<name>.json` — **no environment variables are used**.

### Docker

```bash
docker compose up --build      # MariaDB + Stella server on http://localhost:8080
```

The compose stack is **JSON-only** (no env vars for Stella itself; only MariaDB uses `MARIADB_*`):
- Creates `stella_kfc` and `stella_core` databases (`docker/init-db.sql`).
- Mounts `docker/appsettings.json` over `/app/appsettings.json` and `docker/plugin_*.json` (with `Server=db`) over the app's `plugins/plugin_*.json`.
- `docker/entrypoint.sh` waits for the DB to accept TCP, then `exec dotnet Stella.dll`. It does NOT render config from env.
- Applies EF migrations on first boot (idempotent). Static-data seeding and `music_db.xml` loading are done from the WebUI after first boot.

To change host URLs, toggles, or the WebUI password, edit the mounted JSON files (`docker/appsettings.json`, `docker/plugin_*.json`) and restart the container. To enable `services.get` to advertise a public URL, set `Stella.ServerUrl` in `appsettings.json` (e.g. `http://10.0.1.133:8080/eamuse`).

Each plugin's `PostBuild` target copies its DLL and config into `Stella/bin/Debug/net10.0/plugins/` — preserve this target when adding plugin projects. Use **forward slashes** in `DestinationFolder`/`SourceFiles` paths (e.g. `$(SolutionDir)Stella/bin/Debug/net10.0/plugins/`); literal backslashes are not normalized by the `Copy` task on Linux and create literal `bin\Debug` directories.

## Coding Style & Naming Conventions

- C# with `Nullable` and `ImplicitUsings` enabled; target `net10.0`. No `.editorconfig` exists; follow existing style: 4-space indentation, `PascalCase` for types/public members, `camelCase` for locals/parameters.
- EF contexts live in `EF/`; entities are singular (`Card.cs`). Request/response models in `Models/` follow `<Verb><Subject><Request|Response>` (e.g. `CardInquireRequest`).
- Handlers belong in `Handlers/`, one class per service area. Unified sv6/sv7 handlers branch on the game version derived from the e-amusement `model` string (see `Util/KfcVersion.cs`).

## Plugin & Handler Guidelines

Extend `StellaHandler` and annotate methods with `[StellaHandler(service, module, typeof(Request))]`:

`service`/`module` must match the request's `f` query param (`service.method`). New plugins implement `IStellaPlugin` (`Name`, `Version`, `GameCode`, `OnBuilderInitialize`/`OnAppInitialize`) and ship a `plugin_<name>.example.json`.

### When adding a handler, ask the user which pattern to use

When a new handler covers multiple game versions (e.g. sv6 + sv7), **ask the user** whether the two versions should share logic or be implemented separately. Do not assume — the answer depends on how different the asphyxia code paths are:

- **Shared (unified)** — if asphyxia uses the same handler function for both versions and branches internally on `getVersion(info)`, use one `[StellaHandler]` per version that calls a common `HandleInternal(gameVersion)`:

```csharp
public class MyHandler : StellaHandler
{
    [StellaHandler("game", "sv6_mymethod", typeof(MyRequest))]
    public async Task<MyResponse> Handle() => await HandleInternal(6);

    [StellaHandler("game", "sv7_mymethod", typeof(MyRequest))]
    public async Task<MyResponse> HandleNabla() => await HandleInternal(7);

    private async Task<MyResponse> HandleInternal(int gameVersion)
    {
        var request = Request as MyRequest;
        if (request is null) return new MyResponse { Status = "1" };
        // Branch on gameVersion where asphyxia branches on getVersion(info)
    }
}
```

- **Separate** — if asphyxia uses different handler functions or the v6/v7 logic diverges significantly (different request/response models, different DB schemas, different pug templates), implement each version as its own method with independent logic:

```csharp
public class MyHandler : StellaHandler
{
    [StellaHandler("game", "sv6_mymethod", typeof(MyV6Request))]
    public async Task<MyV6Response> HandleV6()
    {
        var request = Request as MyV6Request;
        if (request is null) return new MyV6Response { Status = "1" };
        // v6-only logic
    }

    [StellaHandler("game", "sv7_mymethod", typeof(MyV7Request))]
    public async Task<MyV7Response> HandleV7()
    {
        var request = Request as MyV7Request;
        if (request is null) return new MyV7Response { Status = "1" };
        // v7-only logic
    }
}
```

Always null-check after `Request as XXXRequest` — deserialization can fail and `Request` will be null.

### Handler completeness — the asphyxia parity rule

Stella aims for byte-level response parity with the asphyxia plugin. This applies to **all** plugins, not just KFC. Every route registered in the corresponding asphyxia plugin MUST have a matching `[StellaHandler]` in Stella. Missing handlers cause the game to log `crypt level not match` or `No handler found` and fail the request.

**Before adding or modifying handlers for any plugin, always cross-check against the asphyxia source:**
- Game-specific plugins: `<plugin>/index.ts` `MultiRoute('method', handler)` or `R.Route(...)`. Note that `MultiRoute` registers `game.method`, `game_2.method`, `game.sv6_method`, `game.sv7_method` — Stella needs at least the `sv6_*` and `sv7_*` variants.
- Core routes: `core/src/eamuse/Core/*.ts` `container.add('service.method', handler)`.
- Stub routes (asphyxia passes `true`): `save_mega`, `play_e`, `play_s`, `frozen`, `exception` — Stella must still register these with a success-only response.
- Inline routes (defined directly in `index.ts` with `send.object(...)`): `shop`, `eventlog.write`, `package.list`, `ins.netlog` — port the exact response shape.

**When porting an asphyxia handler to Stella, verify ALL of the following:**
1. **Route name**: `service.module` matches exactly (case-sensitive). `game.sv7_entry_s` not `game.sv7_entryS`.
2. **Response fields**: every field in the asphyxia `send.object({...})` or pug template must be present in the Stella response model. No extra fields, no missing fields.
3. **Field order**: property declaration order in C# = XML element order on the wire. Must match asphyxia's `send.object({...})` key order or pug template line order.
4. **Field types**: `__type` must match (see "Response XML serialization" below). `u8` vs `s8` vs `u32` — check asphyxia's `K.ITEM('type', value)`.
5. **Conditional fields**: if asphyxia uses `if` guards, decide whether to use nullable types (to omit) or always-render with defaults (to match asphyxia's initialized-to-zero behavior).
6. **Stub handlers**: if asphyxia passes `true` for a route, Stella should return a minimal success response (empty body with `status="0"`).
7. **`services.get`**: `ServicesHandler.GetServices` advertises a fixed core service list **plus** every unique service prefix that has a registered `[StellaHandler]` (discovered dynamically from `PluginService.RegisteredHandlers`). New game plugins with any service prefix (not just `game*`) are advertised automatically — no host edit needed. Only add to the hardcoded core list if a service has no backing handler (e.g. `ntp`, `keepalive`, `numbering`).

**Current handler inventory**: handler count and service prefixes are logged at startup (`Total handlers cached:` / `Cached handler: <service>:<module>`). `services.get` is built dynamically from `PluginService.RegisteredHandlers`, so the advertised list always matches what is actually registered.

### KFC Plugin (EXCEED GEAR + NABLA)

The KFC plugin serves both sv6 and sv7 from unified handler classes. Static game data (events, courses, valgene, apigene, arena, extend, music_limited, ...) is stored in EF tables (`sv_static_*`) and seeded from `Data/Seed/asphyxia_data.json` (an extract of the asphyxia plugin's `data/exg.ts`, `data/nbl.ts`, `data/ii.ts`, `data/booth.ts`). `music_db.xml` (shift_jis, ~8.4MB) is **NOT** committed and is **no longer loaded at startup** — upload it from the KFC WebUI "Data" page (or drop it in `Data/Seed/music_db.xml` and use "Reload from disk") to populate `sv_music`. In Docker, `docker-compose.yml` mounts `StellaKFCPlugin/Data/Seed/music_db.xml` over the app so a new file can be supplied without rebuilding.

The `IDataProvider` abstraction (default: `DbDataProvider`) reads static data from EF tables. (A JSON-based provider is sketched for future use but is not selectable from config today.)

**sv7 (NABLA) static-data merge rule**: asphyxia's `sv7_common` does NOT swap valgene for a NABLA-only set — it merges `VALGENE` (vols 1-18) with `VALGENE7` (vol 19) for both `info` and `catalog` (see `common.ts` `sv7_common`). Most other sv7 static data (events, courses, information, extends, licensed/unlock songs, current arena, apigene) is NABLA-only and replaces the sv6 set. `DbDataProvider.GetValgeneInfo()` / `GetValgeneCatalog()` implement the merge by returning both `Version=6` and `Version=7` rows when `GameVersion >= 7`; all other providers filter by `Version == GameVersion`. When adding a new static-data table that asphyxia merges across versions for sv7, follow the valgene pattern — do NOT blindly filter by `Version == GameVersion`.

`ViiMigrate` (in `Handlers/MigrationHelper.cs`) handles EG→∇ profile migration: copies profile/items/params/scores, remaps clear lamps, recomputes volforce, and resets EX scores for charts Konami reset between versions.

## Response XML serialization — critical rules

The e-amusement protocol uses KBinXML, a binary XML format where every leaf node carries a `__type` attribute (e.g. `u8`, `s32`, `str`) and array nodes carry a `__count` attribute. The game's parser is strict — wrong types, missing fields, wrong field order, or missing `__count` cause `property_node_refer error` logs, `crypt level not match`, or outright crashes.

### Field order MUST match asphyxia's response structure

The game's XML parser expects fields in a specific order. **Always compare your `[XmlElement]` ordering against the asphyxia source** — pug templates (`<plugin>/templates/*.pug`) for template-based responses, or the `send.object({...})` call order in asphyxia handlers for inline responses. C# `XmlSerializer` emits elements in declaration order, so the property order in `Models/*.cs` IS the wire order.

Known field-order-sensitive responses (KFC examples — same principle applies to all plugins):
- **`load`** (`LoadResponse.cs`): must match `kfc/templates/load.pug` lines 59-215 exactly. The order is: `result` → `name` → `code` → `sdvx_id` → `gamecoin_packet` → `gamecoin_block` → `appeal_id` → `last_music_id` → ... → `kac_id` → `skill_*` → `support_team_id` → `weekly_music` → `additional_info` → `ea_shop` → `eaappli` → `cloud` → `block_no` → `skill` → `item` → `present` → `param` → count fields → `arena` → `valgene_ticket` → `creator_item` → `variant_gate`.
- **`common`** (`GetCommonResponse.cs`): must match `kfc/handlers/common.ts` `send.object({...})` key order.
- **`load_m`** (`LoadMResponse.cs`): v6 = 21 params, v7 = 26 params (see asphyxia `loadScore`).

### `__type` and `__count` attributes

`Stella/Util/XDocumentTypeExtensions.cs` (`AddKBinTypesFromResponse`) auto-adds `__type`/`__count` attributes after XML serialization. Known pitfalls:

1. **IP field false-positive**: `GetKBinType` checks if the property name contains "ip" and returns `ip4`. This incorrectly matched `handtrip` (contains "ip" as substring). The check was narrowed to exact `ip`/`gip`/`lip`/`*_ip`/`*ipaddr` suffixes. **When adding new fields, verify the type mapping is correct** — check `GetKBinType` in `XDocumentTypeExtensions.cs`.

2. **`List<int>` arrays need `__count`**: Fields like `over_radar` and `param` use `List<int>` so `XDocumentTypeExtensions` emits `__type="s32" __count="N"`. Using `string` or `int` instead of `List<int>` omits `__count` and crashes the game. Always use `List<int>` for KBinXML array fields.

3. **Empty arrays**: `XDocumentTypeExtensions` now sets `__count=0` even for empty lists. Ensure at least one XML element exists (XmlSerializer skips empty lists — add a dummy `0` entry or make the field non-nullable with a default).

4. **snake_case conversion**: `GetListElementName` uses `ConvertToSnakeCaseElementName` (PascalCase → snake_case). `OverRadar` → `over_radar`, not `overRadar`. Verify the converted name matches the XML element name the game expects.

### Conditional fields

asphyxia pug uses `if` guards (e.g. `if playCount`, `if arena`, `if variant`). In Stella:
- **Count fields** (`play_count`, `day_count`, etc.): asphyxia omits them when 0. Stella always emits them — this is tolerated by the game.
- **`variant_gate`**: asphyxia (profiles.ts L953-964) only initialises a zero-filled `variant` object when `dVersion >= 20250422`; before that datecode a missing record means the pug `if variant` guard omits the element. Stella mirrors this: `LoadResponse.VariantGate` is `VariantGateElement?` and only assigned when `dVersion >= 20250422 || variant != null`.
- **`additional_info`**: asphyxia always renders the wrapper element (inner `pro_team_id` is conditional). Stella must always emit `additional_info`.
- **`arena`, `valgene_ticket`, `creator_item`**: asphyxia conditionally renders these. Stella uses nullable (`?`) types — `XmlSerializer` omits null elements, matching asphyxia's `if` guard.

### Parity audit findings (webui ← parity-audit branch)

These bugs were found by diffing Stella against `/home/user/core` and `/home/user/kfc` and fixed — do not regress them:

- **`load` response has NO `blaster_count`** for sv6/sv7. asphyxia `load.pug` (v>=6) emits `blaster_energy` but not `blaster_count` (blaster_count only appears in the v2 section). `LoadResponse.cs` must not declare `blaster_count`.
- **`valgene_ticket` is conditional** (asphyxia `if valgeneTicket`). `LoadResponse.ValgeneTicket` is `ValgeneTicket?` (nullable) and only assigned when a `sv_valgene_tickets` row exists — do NOT default it to `new()`.
- **`unlock_all_songs` sends EMPTY `music_limited`**: asphyxia `common.ts` L287 is `music_limited: { info: U.GetConfig('unlock_all_songs') ? [] : songs }` — when `unlock_all_songs` is ON, `music_limited.info` is `[]` (the `songs` array built in the unlock block at L142-156 is completely discarded). The game shows all songs without needing limited entries. `BuildMusicLimited` returns an empty `MusicLimitedElement` early when `cfg.UnlockAllSongs` is true. Do NOT emit per-song entries when unlock is on — that creates duplicates and diverges from asphyxia.
- **Core `expire` attribute values must match asphyxia**: `message.get` → 300, `package.list` → 1200, `pcbtracker.alive` → 1200 (Stella previously hard-coded 600 for all three).
- **`cardmng.getrefid` for an existing card** must return that card's `dataid`/`refid` (asphyxia updates the pin and returns the existing refid). Returning an empty response breaks returning card users. `CorePlugin/Handlers/CardHandler.cs` `GetRefId` returns `DataId = RefId = existing.RefId`.
- **`KfcVersion.GetVersion` parses the FULL 10-digit datecode** (e.g. `2026040700`) and compares against 10-digit thresholds, exactly like asphyxia `utils.ts getVersion`. It must NOT strip the trailing 2 digits — that yields an 8-digit value which is always less than the 10-digit thresholds and mis-detects every version as 1 (BOOTH), breaking every bare `game.*` / `game_3.*` route that calls `GetVersion(Model)`. (`GetDateCode` does strip to 8 digits for the YMD datecode — that is correct and separate.)
- **`save_valgene` response** only emits `ticket_num`/`limit_date` when a `sv_valgene_tickets` row exists (asphyxia `saveValgene` guards them with `if valgeneTicket !== null`). `SaveValgeneResponse.TicketNum`/`LimitDate` are nullable (`int?`/`ulong?`).
- **Non-unlock `music_limited` sv7** must include NABLA-original songs (`info.version === '7'`) in addition to `EGSONGS_LOCKED.crossresonance` (asphyxia `common.ts` L226). The `song.Version == 7` condition was missing, dropping every NABLA-original song in non-unlock mode.
- **`sv_music` carries `inf_ver` and `distribution_date`** (parsed from `music_db.xml` `<info>` by `MigrationHelper.LoadMusicDbAsync`, which backfills them on existing rows — old values are snapshotted before overwrite so the update check works). The non-unlock `music_limited` path (`CommonHandler.BuildMusicLimited`) uses them to: skip unreleased songs (`distribution_date > currentYmd`), emit only `music_type=3` for XCD/infinite tracks (`inf_ver == 6`, asphyxia L206), and apply the `info.version == 6`/`== 7` guards. The non-unlock path filters charts via `MigrationHelper.GetMusicDifficulties()` (`difnum != 0`, the closest equivalent to asphyxia's per-version `difficulty[absVersion][diff] != '0'` — Stella's `music_db.xml` has a single difnum per difficulty, not per-version).
- **`services.get` `expire` is 10800** (asphyxia core `index.ts` services.get `@attr.expire`), not 600. Core service items are advertised in asphyxia `coreModules` declaration order (ntp + keepalive first). Stella additionally advertises per-plugin service prefixes dynamically — that is an intentional Stella extension (drop-in plugin discovery), not an asphyxia feature.
- **`common` `extend` MUST include `EXTENDS6`/`EXTENDS7` entries** (kac type 6, demo videos type 21, megamix type 17 id 91..94, blaster-gate type 18), read via `provider.GetExtends()` and filtered by `CheckVerStart(dVersion, MinVersion, StartDate, date)` — asphyxia `common.ts` L75/L102. Previously Stella only emitted megamix (rebuilt from `SvMegamixData`) and dropped every other EXTENDS row even though they were seeded into `sv_static_extend`. Order: EXTENDS first, then information notices (type 1), then Stella's notification banner. `SvMegamixData` is now redundant (megamix comes from `sv_static_extend`) but kept for compatibility.
- **`facility.get` does NOT emit an `expire` attribute** (asphyxia core `facility.get` has no `@attr.expire`). `GetFacilityResponse.Expire` is `int?` (nullable) so XmlSerializer omits it when unset.
- **April Fools songs in `music_limited`** (asphyxia `common.ts` L579-593): on April 1 (`currentDate` starts with `4/1/`, i.e. `currentYmd % 10000 == 401`) the `APRILFOOLSSONGS` list is appended to `songs` AFTER the unlock/non-unlock block, so it appears in `music_limited` ONLY in non-unlock mode (v>=6) — when `unlock_all_songs` is ON, `music_limited` is `[]` so the April Fools entries are also discarded. Each April Fools song emits 5 entries (music_type 0..4, limited:3) with NO difnum filter. asphyxia imports `APRILFOOLSSONGS` only from `data/exg.ts` and uses the same list for sv6 and sv7, so `KfcSeeder` seeds `sv_april_fools_songs` for both version 6 and version 7 from the exg array, and `BuildMusicLimited` appends them via `provider.GetAprilFoolsSongs()` after the non-unlock main loop.

- **`hiscore` seq fields use `SvProfile.Code`, not `IdToCode(PK)`** (asphyxia `features.ts`: `a_sq = IDToCode(profile.id)`). Stella stores the in-game display code directly on `SvProfile.Code` (already `0000-0000`), and `load` returns `sdvx_id = Code`. `HiscoreHandler` must set `AsqSequence`/`LsqSequence`/`AxSqSequence`/`LxSqSequence` to the profile's `Code` — using `KfcVersion.IdToCode(pScore.Id)` formats the EF auto-increment primary key, which is unrelated to the in-game code and would not match the player's `sdvx_id`.
- **`load_r` rival `seq` uses the rival profile's `Code`** (asphyxia `rival`: `seq = IDToCode(p.sdvxID)` where `sdvxID` is the rival profile's id). `LoadHandler.LoadRivalInternal` looks up the rival profile and uses `rivalProfile.Code`, falling back to `IdToCode(r.SdvxId)` only when the profile is missing.
- **Event extends (stamp/completestamp/tama/variant/achmissions)** now match asphyxia `common.ts` L386-485. The events6/7 catalog (`webui/asset/json/events.json`) is seeded into `sv_static_event_list` (from `Data/Seed/events_list.json`); the user toggle (asphyxia `webui/asset/config/events.json [id].toggle`) is `SvEventList.Enabled` (default false — like asphyxia with no config file, no event extends emit until a user opts in). `CommonHandler.AddEventExtends` builds: stamp (type 3, with refillStamps bonus from a `refillStamps` `sv_static_unlock_event` row and select-type sheet header), completestamp (type 19, `JSON.stringify(info.data)`), tama (type 20 + `TAMAADV_ENABLE` event flag), variant (type 22, `minOverTrackRank`/`minSealDiff` from `SvEventList.SettingsJson`), and achmissions (`ACHIEVEMENT_EVENT_MISSION`/`..._PRIORITY` event flags from the `SettingsJson` toggle object). `refillStamps` is now seeded as its own `sv_static_unlock_event` row (EventId=`refillStamps`). The WebUI "Unlock Events" page toggles `SvEventList.Enabled` (the old `ToggleUnlockEvent` wrongly flipped `SvEventData.SortOrder`).
- **`Stella.MigrationHelper` no longer produces `bin/Debug/net10.0/bin/Debug/...` infinite nesting**. The Web SDK re-globbed transitive ProjectReference content (KFC `Data/Seed/*.json` with `CopyToOutputDirectory`) from the output dir on every build, replicating the output inside itself. The csproj now has explicit `<Content Remove="bin/**"/>` / `<None Remove="bin/**"/>` (and obj/plugins) and no longer links `plugin_kfc.json` into the host output (`dotnet ef` uses `StellaKFCContextFactory`, which hardcodes the connection string, so no `plugin_kfc.json` is needed here).

- **`load` present granting for gift events** (asphyxia `profiles.ts` load L820-907). Gift/cross_online events toggled on in the WebUI grant their `EVENT_ITEMS6/7` songs as presents on the next `load`. Two toggle shapes, matching asphyxia `eventConfig[id].toggle`: **boolean** (direct events — `sdvx10thstamp`-style keys where `EVENT_ITEMS[id]` exists directly) use `SvEventList.Enabled` + `checkVerStart(MinVersion, StartDate)`; **object** (prefix events — `xrecord_1_1`/`konasute_1` sub-keys) use `SvEventList.SettingsJson` `{toggle:{subkey:bool}}` + per-subkey `VersionsJson`/`StartsJson` arrays. typeIds: `gift_crew`→type 11 param 1, `gift_ap`→type 1 param 1, `gift`/`cross_online`→type 0 param 23. Items already owned are not re-granted. `LoadHandler.GrantEventPresents` runs BEFORE the `SvItems` query so granted items also appear in the response `item` list. `EVENT_ITEMS6/7` is seeded into `sv_static_event_items`; the events6/7 array `version`/`start` are preserved in `SvEventList.VersionsJson`/`StartsJson`. The WebUI "Unlock Events" page renders per-song sub-toggles for prefix events and a single toggle for direct events. April-Fools yukkuri presents (`5546` type 1, `10244` type 14) are granted when `dVersion >= 20250324` and it is April 1 OR the `aprilyukkuri` startup flag is enabled (asphyxia `flags.json aprilyukkuri` toggle, now modelled in `sv_startup_flag`).

- **Startup flag event strings** (asphyxia `common.ts` L57-77 / L85-99: `flags.json` `[id].toggle` → push `[id].str` to `events`). The `events.json` `flags` array (cuda, submonvsync, konami50, christmas2023, valentines2024, whiteday2024, halloween, aprilrainbowline, aprilyukkuri) is seeded into `sv_startup_flag` from `Data/Seed/startup_flags.json`. `CommonHandler.AddStartupFlagEvents` pushes each enabled flag's event strings into the `events` list before date-based events, matching asphyxia's order (EVENT6/7 base → flags.json toggles → date events → event-extend flags). The WebUI "Startup Flags" page renders toggles for these alongside the plugin config toggles; `saveStartupFlags` persists `flag_<id>` checkbox state to `sv_startup_flag.Enabled`. Date-based event strings (MERRY_CHRISTMAS_2023, etc.) are also added automatically by `AddDateEvents` on the right dates — the flags allow forcing them on out-of-season.

- **`eacoin.consume` field order** must be `autocharge`, `acstatus`, `balance` — matching asphyxia `EamuseCoin.ts` `eacoin.consume` `set(...)` call order. Stella previously declared `balance` first; `ConsumeResponse` property declaration order was reordered to match (C# declaration order = wire order).
- **`common` valgene `catalog` field order** must be `valgene_id`, `rarity`, `item_type`, `item_id` — matching asphyxia `common.ts` L534-538 `valgene_items.push({...})` key order. `ValgenesCatalog` previously had `rarity` last. (The sibling `ApigeneCatalog` already had the correct `apigene_id, rarity, item_type, item_id` order.)
- **`load` `variant_gate` is conditional on datecode** — see the corrected bullet in "Conditional fields" above. The previous "always render" guidance was wrong for pre-20250422 datecodes.
- **`entry_s` response `gip`/`lip` use KBinJSON `4u8` type** (asphyxia `features.ts` globalMatch `K.ITEM('4u8', e.gip)`), NOT `ip4`. `4u8` is a single element with space-separated u8 values and a built-in count of 4 — it must NOT carry a `__count` attribute (adding `__count="4"` makes the KBinXML writer compute 4*4=16 bytes, corrupting the response). `XDocumentTypeExtensions.GetKBinType` maps `gip`/`lip` → `4u8`; `ip4` is reserved for `ip`/`*_ip`/`*ipaddr` names and the value-based `IsValidIPv4` check (used by `facility.get` `globalip`, which sends a dot-separated address string). `LoungeHandler` uses a custom `U8Array : List<int>, IXmlSerializable` that writes `__type="4u8"` and space-joined values with no `__count`.
- **`lounge.wait` is conditional**: asphyxia `features.ts` `lounge` only sends `wait` when there are matching rooms (the `else` branch); the no-matches branch sends only `{ interval: 5 }`. `LoungeResponse.Wait` is `uint?` (nullable) so `XmlSerializer` omits it when no matches exist — do NOT default it to 0.
- **`print` response `genesis_cards`/`after_power` are direct arrays** (asphyxia `profiles.ts` `print` `genesis_cards: genesisCards.map(...)`, `after_power: generatorArray.map(...)`). KBinJSON serialises a direct array as repeated same-named elements (`<genesis_cards>...</genesis_cards>` x N), NOT wrapped in an `<info>` child. `PrintResponse` uses `[XmlElement]` (not `[XmlArray]`/`[XmlArrayItem]`) to reproduce this. The wrapped `<foo><info/>...` shape is only for responses that explicitly nest under an `info` key (e.g. `music_limited: { info: songs }`, `item: { info: [...] }` in pug templates).
- **KBinJSON direct-array vs `info`-wrapped array convention**: asphyxia `send.object({ foo: [...] })` (direct array) → repeated `<foo>` elements. `send.object({ foo: { info: [...] } })` or pug `foo\n each x in xs\n info` → `<foo><info/>...</foo>`. Stella matches via `[XmlElement("foo")] List<T>` for direct arrays, and `[XmlElement("info")] List<T>` inside a `FooElement` for wrapped arrays. Always check which form the asphyxia source uses before choosing `XmlElement` vs a wrapper element.
- **Core responses must carry `status="0"` on the inner element**: asphyxia `send.object(content)` always merges `status` (default 0) into `content['@attr']`, which KBinJSON puts on the root child element (e.g. `<cardmng status="0"/>`, `<package status="0"/>`, `<pcbtracker status="0"/>`). Stella response models must declare `[XmlAttribute("status")] string Status = "0"`. `CardGetRefIdResponse`, `GetPackageListResponse`, and `GetPcbTrackerResponse` previously omitted it.
- **`pcbtracker.alive` `ecenable` is an integer `1`/`0`** (asphyxia `PCBTracker.ts` `CONFIG.enable_paseli ? 1 : 0`), not a boolean. `GetPcbTrackerResponse.ECEnable` is `int` (serialised as `ecenable="1"`); a C# `bool` would emit `ecenable="true"`, which diverges from asphyxia and may confuse the game.
- **Nullable VALUE types (`int?`, `ulong?`, etc.) in response models break KBinXML**: `XmlSerializer` renders a null `int?` as `<foo xsi:nil="true" />` (NOT omitted), and the `xsi:` namespace prefix contains a `:` which KBinXML cannot encode — the binary output is corrupted and the game/client gets "The ':' character cannot be included in a name". Reference types (`string?`, `SomeClass?`) ARE omitted when null (no `xsi:nil`), so they are safe. `Program.cs` strips `xsi:nil` elements and namespace-declaration attributes from the `XDocument` before KBinXML encoding as a safety net, but the intended pattern for conditional VALUE-type fields is to keep them non-nullable and rely on the `xsi:nil` stripping, or use the `ShouldSerialize{Prop}` pattern. `LoadResponse.SupportTeamId` (`int?`) and `AdditionalInfoElement.ProTeamId` (`string?`) rely on this — `support_team_id`/`pro_team_id` are omitted when the BPL condition is false, matching asphyxia `load.pug` `if bplSupport > 0 && !bplPro` / `if bplPro && bplSupport > 0`.
- **`load` item processing order** must match asphyxia `profiles.ts` L993-1002: navigators → appeal cards → `removeStampItems` → appeal parts (v7) → generator power (type 7, id 0..49) **last**. Stella previously added generator power first. The functions all mutate the same `items` list in place (asphyxia's reassignment bug is harmless because `items` is the same array reference), so both unlocks apply simultaneously — Stella mirrors this by appending to one `items` list.
- **`load` `pro_team_id` uses the `% 10` display value**: asphyxia reassigns `profile.bplSupport = profile.bplSupport % 10` before the pug renders, so `pro_team_id` = `bplSupport % 10` (1..9), NOT the raw `bplSupport`. `pro_team_id` is only emitted when `bplPro && (bplSupport % 10) > 0`. `LoadHandler` uses `bplSupportDisp` (the `% 10` value) for both the condition and the value.
- **`load` `weekly_music` ranking**: asphyxia `profiles.ts` L953-968 queries `getRankListDB` (webui.ts) for the active weekly music across difficulties 0..4 and emits the requester's rank entry per difficulty (`week_id`, `music_id`, `music_type`, `exscore`, `rank`). `LoadHandler.BuildLoadWeeklyMusic` mirrors this using the `sv_static_weekly_music` + `sv_weekly_music_score` tables (only when `dVersion >= 20241210` and an active week exists).

### Fields NOT in asphyxia responses

Do NOT add fields that asphyxia doesn't send. The game's parser may reject unknown elements. For example, `extrack_energy` was in an early Stella `LoadResponse` but is NOT in the asphyxia pug — it was removed to match.

### Request deserialization — space-separated arrays

asphyxia uses `$(data).numbers('field')` to read space-separated integer arrays from a single XML element (e.g. `<gip>172 19 0 1</gip>`). C# `XmlSerializer` cannot deserialize this into `List<int>` directly — it expects separate `<gip>` elements per value.

**For request models with space-separated integer arrays** (e.g. `gip`, `lip` in `entry_s`):
- Declare the field as `string` with `[XmlElement]`.
- Add a `[XmlIgnore]` computed property that parses the string into `List<int>`:

```csharp
[XmlElement(ElementName = "gip")]
public string GipRaw { get; set; } = "";

[XmlIgnore]
public List<int> Gip => GipRaw.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
```

**For response models** that need `__count`/`__type` on arrays (e.g. `over_radar`, `param`), use `List<int>` so `XDocumentTypeExtensions` emits the correct attributes. The response side is handled by the serializer, not `XmlSerializer` directly.

### Request deserialization — root element name normalization

asphyxia parses the request XML into a generic JSON object and reads the `call` element's child name as the module name (e.g. `game_3`, `game_2`, `game`). It does **not** care about the XML root element name — the data is accessed by path (`get(body.data, 'call.${body.module}')`).

Stella uses `XmlSerializer` which **does** care about the root element name. Request models declare `[XmlRoot(ElementName = "game")]`, but older games (e.g. GRAVITY WARS sv3) send `<game_3>` as the root element. This causes `XmlSerializer` to throw `"<game_3 xmlns=''> was not expected"`.

**Solution**: `PluginService.NormalizeRootElementName()` renames the XML root element to match the request type's `[XmlRoot]` ElementName before deserialization. This is called automatically for every request — no action needed when adding new handlers.

### Request routing — legacy `module`+`method` vs modern `f` parameter

Older game versions (e.g. GRAVITY WARS sv3) send routing via query params `module=services&method=get` instead of the modern `f=services.get`. Stella's `Program.cs` checks `f` first, then falls back to `module`+`method`. Both route groups (`/eamuse` and `/core`) support this.

### EAMUSE request detection — User-Agent variants

The middleware (`EAmuseXrpcInputMiddleware`) checks the `User-Agent` header to identify e-amusement requests. Modern games send `EAMUSE.XRPC/1.0`, but older games (e.g. GRAVITY WARS sv3) send `EAMUSE.Httpac/1.0`. Both are accepted. When adding support for other game versions, verify the User-Agent and add it to `IsEAmuseRequest` if different.

### Multi-version route registration

asphyxia's `MultiRoute` registers each method under multiple prefixes: `game.<method>`, `game_2.<method>`, `game.sv6_<method>`, `game.sv7_<method>`. The game client determines which prefix to use based on its version:

- **sv6 (EXCEED GEAR)**: `game.sv6_common`
- **sv7 (NABLA)**: `game.sv7_common`
- **sv3 (GRAVITY WARS)**: `game_3.common` (note: asphyxia does NOT register `game_3` via `MultiRoute`, but the game sends it anyway)
- **sv1 (BOOTH) / sv2 (infinite infection)**: `game.common` or `game_2.common` (bare routes)

Stella must register **all** route prefixes that the game might send. For KFC this means: `game.sv6_*`, `game.sv7_*`, `game.*` (bare), and `game_3.*` for every handler method. Each bare/`game_3` variant calls the same internal method with the version auto-detected from the model string via `KfcVersion.GetVersion(Model)`.

## Database & Configuration

- MariaDB/MySQL via Pomelo. Each plugin registers its own `DbContext` and connection string from its `plugin_<name>.json` `db` field (JSON-only; **no env vars**).
- `StellaKFCContext.OnConfiguring` caches the connection string/server version once (lazy, via `ResolveConfiguration`) so `new StellaKFCContext()` does not re-read config on every request. The connection string is resolved by `ResolvePluginConfigPath("plugin_<name>.json")` (working dir `plugins/`, then `AppContext.BaseDirectory/plugins/`).
- `StellaKFCContextFactory` (in `Stella.MigrationHelper`) provides a design-time factory for `dotnet ef` so migrations can be generated without a live DB.
- Both `plugin_*.example.json` (template) and `plugin_*.json` (self-hosted defaults) are tracked. Docker variants live in `docker/`. Don't put real production secrets in the tracked self-hosted defaults — edit a mounted copy instead.

## WebUI

Stella ships an optional admin WebUI at `/webui` (dark theme, Tailwind, pastel-pink/magenta accent). It is server-rendered Razor (NOT a SPA) and is designed to be extended by plugins, not just the host. Enabled by `WebUI.Enabled` in `appsettings.json`; sign in with `WebUI.Password` (cookie auth scheme `StellaWebUI`, 12h sliding). Everything under `/webui` is separate from the e-amusement `/eamuse` + `/core` POST routes.

### Host infrastructure

- `Stella/WebUI/WebUIServiceExtensions.cs` — `AddStellaWebUI(plugins)` (cookie auth, antiforgery header `X-CSRF-TOKEN`, `RazorRuntimeCompilation`, per-plugin embedded-views file providers + compile references) and `MapStellaWebUI(app, plugins)` (Razor Pages, per-plugin static assets at `/webui/static/<pluginId>/...`, and the AJAX endpoint `POST /webui/api/emit/<pluginId>/<event>` which is auth + antiforgery protected).
- `Stella/WebUI/PluginViewRenderer.cs` (`IPluginViewRenderer`) renders an arbitrary view by virtual path to a string via `IRazorViewEngine.GetView`. The host's generic pages delegate plugin page/profile rendering to the plugin.
- `Stella/Pages/` — host Razor Pages: `Login`/`Logout`, `Index` (dashboard), `Profiles`/`Profile`, and the delegating pages `PluginPage` (`@page "/webui/p/<pluginId>/<slug>"`) and `ProfileTab` (`@page "/webui/profile/<refid>/<pluginId>/<slug>"`). `Shared/_Layout.cshtml` is the dark/pink sidebar.
- `Stella/wwwroot/webui/css/app.css` is a committed Tailwind build; `Stella/wwwroot/webui/js/app.js` provides `Stella.emit(pluginId, event, payload)` / `Stella.emitForm(...)` helpers that attach the antiforgery header. Tailwind source lives in `Stella.WebUI.Assets/` (npm); regenerate with `npm run build` there — the built CSS is committed (the ii/asphyxia pattern) so `dotnet build` stays the only build command.

### Plugin WebUI contract (`Stella.Abstractions`)

Plugins extend the WebUI through default-interface members on `IStellaPlugin` (no-op by default) so a plugin opts in only if it wants a UI:
- `WebUIPages` / `ProfilePages` — return `WebUIPageDescriptor`s (`Title`, `Slug`, `View`, `Icon`). Slugs are unique within the plugin and map to `/webui/p/<pluginId>/<slug>` and `/webui/profile/<refid>/<pluginId>/<slug>`.
- `RegisterWebUIEvents(WebUIEventRegistry)` — register `WebUIEventHandler` callbacks for AJAX events invoked from the page via `Stella.emit("<pluginId>", "<event>", {...})`.
- `RenderWebUIPageAsync(slug, services)` / `RenderProfileTabAsync(slug, refid, services)` — return the rendered HTML for a page (query the plugin `DbContext`, build a view model, render via `IPluginViewRenderer`).
- `IStellaGamePlugin` (extends `IStellaPlugin`) adds `GetProfileSummariesAsync` / `GetProfileDetailAsync` so the host `Profiles` page can list game profiles without a host→plugin project reference. `Stella.Abstractions/Cards/IStellaCardProvider` + `StellaCardProviderRegistry` let the host resolve cards via CorePlugin without a reference.

### How plugin views are compiled (read this before adding a page)

Plugin `.cshtml` views are **embedded** in the plugin DLL (`Views/**/*.cshtml` + `<GenerateEmbeddedFilesManifest>true</GenerateEmbeddedFilesManifest>` in the plugin `.csproj`) and **runtime-compiled** by ASP.NET Core. Because plugins are loaded at runtime from `plugins/` (not part of the app's dependency context), the host registers each plugin's embedded views under `/Plugins/<pluginId>/...` via a `PrefixedFileProvider` on `MvcRazorRuntimeCompilationOptions.FileProviders`, and adds the plugin DLL path to `MvcRazorRuntimeCompilationOptions.AdditionalReferencePaths` (via `PluginService.AssemblyPaths`) so the runtime Razor compiler can resolve plugin namespaces.

**Critical gotcha — plugin views MUST use `@model object` + `dynamic`, not `@model <PluginType>`:**

The runtime-compiled view assembly is loaded into the default `AssemblyLoadContext` and references the plugin assembly by name. Resolving the view's base class `RazorPage<TModel>` at type-load time fails with `TypeLoadException: Could not resolve type 'AspNetCore._Plugins_..._<Page>'` when `TModel` is a plugin type, because the plugin assembly (loaded from a byte array) is not part of the app's default reference set at binder time. To avoid this, every plugin view declares:

```cshtml
@model object
@{ dynamic m = Model; }
... @m.SomeProperty ...
```

and accesses the view model through `dynamic` (late-bound; resolves against the real plugin types at render time). **Never** write `@model StellaKFCPlugin.WebUI.ViewModels.XModel` in a plugin view — it compiles but throws `TypeLoadException` on first render.

A second gotcha: a plugin whose top-level class shares the assembly's root namespace (e.g. `public class StellaKFCPlugin` in `namespace StellaKFCPlugin`) collides with that namespace. Do **not** put `@using <RootNamespace>` in `_ViewImports.cshtml` — reference nested namespaces only (`@using StellaKFCPlugin.WebUI`, `@using StellaKFCPlugin.WebUI.ViewModels`). In C# source, reference plugin types via the `using` alias (e.g. `StellaKFCContext.ResolvePluginConfigPath(...)`) rather than the fully-qualified `<RootNamespace>.<SubNamespace>.<Type>` form, which can bind `<RootNamespace>` to the class.

### Adding a WebUI page to a plugin

1. Add a `WebUIPageDescriptor` (or `ProfilePage`) to the plugin's `WebUIPages`/`ProfilePages` with a unique `Slug` and a `View` filename (e.g. `MyPage.cshtml`).
2. Add `Views/Pages/MyPage.cshtml` starting with `@model object` + `@{ dynamic m = Model; }`. (The view is embedded automatically by the `Views/**` glob in the plugin `.csproj`.)
3. Add a `RenderMyPage` branch in the plugin's page renderer (`KfcWebUIPageRenderer` for KFC) that builds a view model and renders `/Plugins/<PluginId>/Pages/MyPage.cshtml` via `IPluginViewRenderer`.
4. If the page needs AJAX, register a `WebUIEventHandler` in `RegisterWebUIEvents` and call it from the view with `Stella.emit("<PluginId>", "<event>", {...})`.
5. Rebuild — the embedded-views manifest is regenerated on `dotnet build`.

### KFC WebUI (`StellaKFCPlugin/WebUI/`)

- `KfcWebUIPageRenderer` — renders 6 top-level pages (Data, Songs List, Startup Flags, Unlock Events, Update WebUI Assets, Weekly Score Attack) and 8 profile tabs (Detail, Score, Skill, Achievements, Rivals, Customization, Valkyrie Generator, Premium Generator).
- `KfcWebUIEvents` — AJAX handlers: `seedStatic`, `loadMusicDb`, `uploadMusicDb` (base64), `saveStartupFlags` (writes `plugin_kfc.json` via `System.Text.Json` with `SnakeCaseLower` and refreshes the live `PluginConfig`), `toggleUnlockEvent`, `updateProfile`. The config path is resolved with `StellaKFCContext.ResolvePluginConfigPath("plugin_kfc.json")` (NOT a hardcoded CWD path) so it works under both `dotnet run` and Docker. Each config property carries matching `[ConfigurationKeyName]` **and** `[JsonPropertyName]` (same key), so the WebUI round-trip preserves the exact key names `IConfiguration` binds at startup (e.g. `maintenance`, `use_blasterpass`) — no key drift after editing toggles.
- `KfcWebUISeeder` — wraps `KfcSeeder.Seed` + `MigrationHelper.LoadMusicDbAsync` + an uploaded-`music_db.xml` saver. This is the **only** place static data and `music_db.xml` are loaded now — startup no longer seeds.
- View models live in `StellaKFCPlugin/WebUI/ViewModels.cs` (records like `SongRow`, `EventRow`). Views are `@model object` + `dynamic` per the gotcha above.

### WebUI config (JSON-only)

`appsettings.json`:

```json
"WebUI": { "Enabled": true, "Password": "stella" }
```

`Stella:ServerUrl` / `Stella:ServerHost` / `Stella:KeepaliveUrl` in `appsettings.json` feed `services.get` and the keepalive URL. There are no `STELLA_*` env vars. For Docker, `docker/appsettings.json` and `docker/plugin_*.json` are mounted over the app.

## Testing Guidelines

No automated test suite exists yet. `TestClient/` is a manual harness. When adding tests, prefer xUnit with a `*.Tests` project referencing `Stella.Abstractions`.

## Commit & Pull Request Guidelines

Commit messages are short, lowercase, imperative summaries (e.g. `update readme`, `nabla dummy support`). Follow that style.

- Open merge requests against `main` with a description referencing the affected handler/plugin or game code.
- Verify `dotnet build Stella.slnx` succeeds and your plugin appears in `Stella/bin/Debug/net10.0/plugins/` before review.
## Games

### SOUND VOLTEX (KFC)

KFC는 SOUND VOLTEX 시리즈의 게임 코드입니다. 각 버전은 내부적으로 다음과 같이 구분됩니다:

| 버전 | 코드 | 이름 | 비고 |
|------|------|------|------|
| 1 | sv1 | BOOTH | |
| 2 | sv2 | infinite infection | |
| 3 | sv3 | GRAVITY WARS | `game_3.*` 라우트 사용 |
| 4 | sv4 | HEAVENLY HAVEN | |
| 5 | sv5 | VIVID WAVE | |
| 6 | sv6 | EXCEED GEAR | `game.sv6_*` 라우트 사용 |
| 7 | sv7 | NABLA | `game.sv7_*` 라우트 사용 |

**버전 감지**: `KfcVersion.GetVersion(Model)`이 e-amusement 모델 문자열의 datecode를 기반으로 버전을 반환합니다.

**라우트 접두사**:
- sv6: `game.sv6_*` (예: `game.sv6_common`)
- sv7: `game.sv7_*` (예: `game.sv7_common`)
- sv3: `game_3.*` (예: `game_3.common`)
- sv1/sv2: `game.*` 또는 `game_2.*` (bare routes)

**구현 참고**:
- sv6/sv7: Stella의 기존 구현 (`Handlers/LoadHandler.cs`, `Handlers/SaveHandler.cs` 등)
- sv3: `https://github.com/22vv0/asphyxia_plugins/tree/kfc` 브랜치의 `game_3.*` 관련 코드만 참고 (sv6/sv7 코드와 충돌 방지)
- **`common` `skill_course` duplicates normal courses for G/H cabinets with God courses** (asphyxia `common.ts` L645-690). asphyxia reassigns `courseData = courseData.concat(godCourses)` and then `acc = acc.concat(courseData)`, so when the cabinet is `G`/`H`, `hasGod === 1`, and `version >= 20230530`, the normal courses for that season are appended a SECOND time before the God courses. Net per-season ordering for the God condition: `[normal, normal, god]`; seasons without `hasGod===1` contribute normals exactly once. `CommonHandler.BuildSkillCourses` reproduces this quirk: it builds the normal-course list once, adds it, then (when the God condition holds) re-adds the same normal list and adds the God-course list. Verified: B cabinet → 176 info all `skill_type=0`; G/H cabinet → 484 info (330 `skill_type=0` + 154 `skill_type=hasGod`). `sv7_common` returns an empty `skill_course` because `COURSES7 = []` in asphyxia (`data/nbl.ts`), so the seeder stores no Version=7 course rows — `DbDataProvider.GetCourses` filters by `Version == GameVersion` and correctly returns empty for sv7.
- **Request `__count` array deserialization requires `List<int>`, not a single `string`** (round 19). `PluginService.PreprocessXmlForArrays` expands every KBinJSON `__count` array (`<foo __type="s32" __count="N">1 2 3</foo>`) into N separate `<foo>` elements before `XmlSerializer` runs, so a request model field declared as `string` only captures the FIRST value (the others are dropped). The old string + `[XmlIgnore] List<int>` computed-property pattern is therefore wrong for any `__count` array; declare the field directly as `List<int>` (or `List<uint>`/`List<string>`) so XmlSerializer collects every expanded element. `SaveVariantGate.OverRadar` and `Sv3ParamInfo.Param` were migrated to `List<int>`. `SaveParamInfo.Params` already used `List<int>` and is the reference pattern.
- **`PreprocessXmlForArrays` must remove empty `__count` arrays** (round 19). A `__count="0"` (or whitespace-only) element is not expanded (no values to split) and was previously left in place as `<foo __type="s32" __count="0"></foo>`, which then made `XmlSerializer` throw `FormatException` parsing `""` into the target `List<int>`. The preprocessor now removes such empty array elements so the list stays empty. Verified: save with `over_radar __count="3"` → DB `"1 2 3"`; `__count="0"` → no crash, list empty; absent `variant_gate` → no variantpower upsert (asphyxia `for (const vp of $(data).elements('variant_gate'))` also skips).
- **`SaveVariantGate` request model is now nullable** (round 19). asphyxia only processes `variant_gate` when the element is present (`$(data).elements('variant_gate')` returns an empty iterable otherwise). `SaveModel.VariantGate` was `= new()` (always non-null), so Stella always performed the `$inc`/`$set` upsert even when the game omitted `variant_gate` (notably older sv6 datecodes). It is now `SaveVariantGate?` so the `if (request.VariantGate is not null)` guard matches asphyxia's element-presence semantics. Modern sv6/sv7 always send `variant_gate`, so this only affects older datecodes.
- **`buy` request uses FLAT `__count` arrays under `<item>`, NOT `<item><info>...</info></item>`** (round 20). asphyxia `profiles.ts` L1130-1162 reads `$(data).numbers('item.item_type')`, `item.item_id`, `item.param`, `item.price` — i.e. `item_type`/`item_id`/`param`/`price` are DIRECT children of `item` (lodash `get` path `item.item_type`), each a KBinJSON `__count` array, and zips the four arrays with `_.zipWith`. Stella's `BuyItemElement` previously modelled `<item><info><item_type>..</item_type>...</info></item>` (info-wrapped), so the game's flat-array request deserialised to an EMPTY item list — items were never saved and `cost` was 0 (no currency deducted). `BuyItemElement` now declares `List<int> ItemTypes`, `List<uint> ItemIds`, `List<uint> Params`, `List<int> Prices` (each `[XmlElement]`); `PreprocessXmlForArrays` expands each `__count` array into repeated elements so the lists collect every value in order. `BuyInternal` zips the four lists (length = min) and `cost = Prices.Sum()`, matching asphyxia. Verified: flat `__count=2` buy saves both items and deducts `sum(price)` from the chosen currency; empty/`__count=0`/absent `item` are safe. The info-wrapped form (`<item><info>...`) is NOT what the game sends for sv6/sv7 — `saveValgene` (`$(data).elements('item.info')`) and save `param` (`$(data).elements('param.info')`) ARE info-wrapped and Stella models them correctly; only `buy` is flat.
- **`facility.get` response was COMPLETELY BROKEN by a nullable `int?` `XmlAttribute`** (round 21). Round 5 made `GetFacilityResponse.Expire` an `int?` with `[XmlAttribute("expire")]` intending XmlSerializer to omit it when unset (asphyxia `facility.get` does not emit `expire`). But XmlSerializer **cannot reflect a nullable value type as an `XmlAttribute`** — it throws `InvalidOperationException: "XmlAttribute/XmlText cannot be used to encode complex types"` while constructing the serializer, so EVERY `facility.get` response failed and the client received a 0-byte body. The `Expire` property is removed entirely (asphyxia `Facility.ts` emits only `status`, which `send.object` always merges). Additionally `FacilityHandler` now sets `Status = "0"` on the response (asphyxia `send.object` defaults `status` to 0 and merges it into `@attr`); previously `GetFacilityResponse.Status` had no default and the handler never set it, so the `<facility>` element carried no `status` attribute. General rule: never declare a nullable value type (`int?`, `uint?`, `bool?`, …) with `[XmlAttribute]` — XmlSerializer rejects it at reflection time. Use a non-nullable type with a `ShouldSerialize{Prop}` method, or omit the property.

### Parity audit — remaining legacy gaps (documented, not fixed)

These are the only known areas where Stella's response/behaviour does NOT match asphyxia. They are all legacy/out-of-scope for the actively-used sv6/sv7 + core paths and cannot be verified without source or test data, so they are left as-is rather than risk introducing untestable bugs:

- **sv1 (BOOTH) / sv2 (infinite infection) `common`**: asphyxia `common.ts` returns a COMPLETELY DIFFERENT response shape for these versions. sv1 (L113-145): `{ limited:{music:[…]}, event:{info:[…]}, catalog:{info:[…]}}` with `songNum=187`, `EVENT`/`SDVX_STATION` from `data/booth.ts`. sv2 (L290-318): `{ event, music_limited, skill_course }` where `skill_course` uses a DIFFERENT field set (`course_id`, `course_name`, `level` [not `skill_level`], `skill_name_id`, `matching_assist`, `gauge_type`, `paseli_type`, `track`) — no `skill_type`/`clear_rate`/`avg_score`. Stella's bare `game.common`/`game_2.common` route calls `BuildCommon(abs(GetVersion))`, which builds the sv6-style response (valgene/arena/extend/music/music_limited/skill_course/weekly_music/apigene) — wrong for sv1/sv2. No BOOTH/infinite-infection client exists to test against, and the booth/ii seed data isn't loaded, so porting these would be untestable. The bare routes are registered only so the server doesn't 404; if BOOTH/infinite-infection support is ever needed, port the sv1/sv2 branches of `common.ts` into separate response models and seed `EVENT`/`SDVX_STATION`/`EVENT2`/`MUSIC_LIMITED`/`COURSES2`.
- **sv3 (GRAVITY WARS)**: asphyxia's sv3 handlers live on a SEPARATE git branch (`https://github.com/22vv0/asphyxia_plugins/tree/kfc`), not in `/home/user/kfc`. Stella has `Sv3LoadHandler`/`Sv3NewHandler`/`Sv3SaveHandler` but they are UNVERIFIED against asphyxia (no source in this tree). `gw_gene`/`gw_mission`/`gw_mission_skipmatch` config flags exist and are shown in the WebUI startup-flags page, but no sv3 handler reads them yet — wiring them requires the sv3 asphyxia source.
- **WebUI `preGeneRoll`/`preGeneReward` (premium generator roll)**: asphyxia's WebUI rolls apigene rewards server-side (`handlers/webui.ts`). Stella's `ProfilePremiumGenerator.cshtml` page only LISTS the apigene catalog and explicitly notes the roll logic is not implemented. This is a WebUI-only convenience feature, NOT part of the e-amusement protocol (the game itself drives the generator via `save_valgene`/`print`), so it does not affect game-side response parity.
- **`getVersion` 4-day gap**: asphyxia returns 0 for datecodes `2025122000`–`2025122399`; Stella returns 7. Edge case with no practical impact (no real datecode falls in that range).

Everything else — all 24 KFC `MultiRoute` methods (sv6/sv7/bare/`game_3`) and all core routes (`message.get`, `package.list`, `pcbevent.put`, `tax.get_phase`, `dlstatus.progress`, `posevent.income.sales.sale`, `eacoin.checkin/checkout/consume`, `facility.get`, `cardmng.inquire/getrefid/authpass/bindmodel`, `pcbtracker.alive`, `eventlog.write`, `ins.netlog`, `services.get`) — has been audited against `/home/user/core` + `/home/user/kfc`, response field order/types/values verified, and bugs fixed in rounds 1–21.
- **`load` `kac_id` must equal the profile NAME, not a separate KAC id** (round 22). asphyxia `load.pug` L85 renders `kac_id(__type="str") #{name}` where `name` is `profile.name` (the pug render call at `profiles.ts` L1007 spreads `...profile`, so `name` = `profile.name`). asphyxia does not track a separate KAC id field — `kac_id` always equals the player's profile name. Stella previously set `LoadResponse.KacId = profile.KacId` (a separate `sv_profile.kac_id` column seeded to `"VOLTEX"` by `NewHandler`), so `kac_id` diverged from asphyxia whenever the player's name differed from `"VOLTEX"`. `LoadHandler` now sets `KacId = profile.Name` so `kac_id == name` byte-for-byte. The `sv_profile.kac_id` column is retained (migrations/seed depend on it) but is no longer read for the load response. Verified: with `name=TARO`, both `<name>` and `<kac_id>` return `TARO`.
