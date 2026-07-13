# Repository Guidelines

Stella is an unofficial implementation of Konami's e-amusement server: a .NET 10 ASP.NET Core web app with a plugin-based architecture. This guide describes how to contribute.

## Project Structure & Module Organization

The solution (`Stella.slnx`) contains:

- `Stella/` — Host web server. Entry point `Stella/Program.cs`; routes under `eamuse/` and `core/`. Holds `Middleware/`, `Services/` (`PluginService`), `Util/` (RC4, LZ77, KBinXML), and `Data/` JSON assets.
- `Stella.Abstractions/` — Shared contracts: `IStellaPlugin`, `StellaHandler`, `StellaHandlerAttribute`, request/response interfaces.
- `CorePlugin/` — Built-in plugin for core handlers (card, facility, eacoin, pcb, services, eventlog). Layout: `Handlers/`, `Models/`, `EF/` (context), `Migrations/`.
- `StellaKFCPlugin/` — Game-specific plugin (KFC code), same layout as `CorePlugin`.
- `Stella.MigrationHelper/` — Helper for generating EF Core migrations.
- `KBinXml.Net/` — Vendored binary XML serializer.
- `TestClient/` — Console client for manual validation.

Plugins load at runtime from the `plugins/` directory and read config from JSON files such as `plugin_core.json`.

## Build, Test, and Development Commands

```bash
dotnet build Stella.slnx       # Build the full solution
dotnet run --project Stella    # Run server (listens on http://+:80)
dotnet test                    # No test project currently exists
dotnet ef migrations add <Name> --project CorePlugin --startup-project Stella.MigrationHelper
```

Each plugin's `PostBuild` target copies its DLL and config into `Stella/bin/Debug/net10.0/plugins/` — preserve this target when adding plugin projects.

## Coding Style & Naming Conventions

- C# with `Nullable` and `ImplicitUsings` enabled; target `net10.0`. No `.editorconfig` exists; follow existing style: 4-space indentation, `PascalCase` for types/public members, `camelCase` for locals/parameters.
- EF contexts live in `EF/`; entities are singular (`Card.cs`). Request/response models in `Models/` follow `<Verb><Subject><Request|Response>` (e.g. `CardInquireRequest`).
- Handlers belong in `Handlers/`, one class per service area.

## Plugin & Handler Guidelines

Extend `StellaHandler` and annotate methods with `[StellaHandler(service, module, typeof(Request))]`:

```csharp
public class MyHandler : StellaHandler
{
    [StellaHandler("myservice", "mymethod", typeof(MyRequest))]
    public async Task<MyResponse> Handle()
    {
        var request = Request as MyRequest;
        // ...
    }
}
```

`service`/`module` must match the request's `f` query param (`service.method`). New plugins implement `IStellaPlugin` (`Name`, `Version`, `GameCode`, `OnBuilderInitialize`/`OnAppInitialize`) and ship a `plugin_<name>.json`.

## Database & Configuration

- MariaDB/MySQL via Pomelo. Each plugin registers its own `DbContext` and connection string from its JSON config.
- Run the server once so plugins call `Database.Migrate()`. Author migrations via `Stella.MigrationHelper`.
- Don't commit secrets — default configs use the `stella/stella` placeholder; override locally.

## Testing Guidelines

No automated test suite exists yet. `TestClient/` is a manual harness. When adding tests, prefer xUnit with a `*.Tests` project referencing `Stella.Abstractions`.

## Commit & Pull Request Guidelines

Commit messages are short, lowercase, imperative summaries (e.g. `update readme`, `nabla dummy support`). Follow that style.

- Open merge requests against `main` with a description referencing the affected handler/plugin or game code.
- Verify `dotnet build Stella.slnx` succeeds and your plugin appears in `Stella/bin/Debug/net10.0/plugins/` before review.
