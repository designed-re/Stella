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
7. **`services.get`**: if you add a new game route, also add it to `ServicesHandler.cs` so the game knows the endpoint exists.

**Current handler inventory** (66 total): see `ServicesHandler.cs` for the full `services.get` list.

### KFC Plugin (EXCEED GEAR + NABLA)

The KFC plugin serves both sv6 and sv7 from unified handler classes. Static game data (events, courses, valgene, apigene, arena, extend, music_limited, ...) is stored in EF tables (`sv_static_*`) and seeded from `Data/Seed/asphyxia_data.json` (an extract of the asphyxia plugin's `data/exg.ts`, `data/nbl.ts`, `data/ii.ts`, `data/booth.ts`). `music_db.xml` (shift_jis, ~8.4MB) is loaded into `sv_music` at startup — it is NOT committed; provide it via docker-compose volume mount or place it in `Data/Seed/`.

The `IDataProvider` abstraction (default: `DbDataProvider`) reads static data from EF tables. A JSON-based provider can be selected via the `STELLA_KFC_DATA_MODE=json` env var (reserved for future use).

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
- **`variant_gate`**: asphyxia always renders it (initializes to zeros if no DB record). Stella must do the same — use a non-nullable default, not `?` optional.
- **`additional_info`**: asphyxia always renders the wrapper element (inner `pro_team_id` is conditional). Stella must always emit `additional_info`.
- **`arena`, `valgene_ticket`, `creator_item`**: asphyxia conditionally renders these. Stella uses nullable (`?`) types — `XmlSerializer` omits null elements, matching asphyxia's `if` guard.

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
