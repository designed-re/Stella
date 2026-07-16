# StellaKFCPlugin Handlers

Unified handlers serving both Sound Voltex EXCEED GEAR (sv6_*) and NABLA (sv7_*).
Each handler method branches on the game version derived from the e-amusement
`model` string (see `Util/KfcVersion.cs`).

## Files
- `CommonHandler.cs` — `sv6_common` / `sv7_common`: events, courses, valgene,
  apigene (v7), arena, extend, music_limited, weekly_music. Ported from
  asphyxia `handlers/common.ts`.
- `LoadHandler.cs` — `sv6_load` / `sv7_load`, `sv6_load_m` / `sv7_load_m`,
  `sv6_load_r` / `sv7_load_r`. Ported from asphyxia `handlers/profiles.ts`
  (`load`/`loadScore`/`rival`) + `templates/load.pug`.
- `SaveHandler.cs` — `sv6_save` / `sv7_save`, `sv6_save_m` / `sv7_save_m`,
  `sv6_save_c` / `sv7_save_c`, `sv6_save_e` / `sv7_save_e`,
  `sv6_save_valgene` / `sv7_save_valgene`, `sv6_save_pb` / `sv7_save_pb`.
- `NewHandler.cs` — `sv6_new` / `sv7_new`. Triggers `ViiMigrateAsync` when a v6
  profile exists and v7 is requested.
- `HiscoreHandler.cs` — `sv6_hiscore` / `sv7_hiscore`.
- `LoungeHandler.cs` — `sv6_lounge` / `sv7_lounge`.
- `PlayHandler.cs` — `sv6_play_s` / `sv7_play_s`, `sv6_play_e` / `sv7_play_e`.
- `FrozenHandler.cs` — `sv6_frozen` / `sv7_frozen`.
- `MigrationHelper.cs` — `ViiMigrateAsync` (EG→∇ profile/items/params/scores
  migration) and `LoadMusicDbAsync` (parse `Data/Seed/music_db.xml` into
  `SvMusic`).

## Data sources
Static game data (events, courses, valgene, arena, ...) lives in EF tables
(`sv_static_*`), seeded at startup from `Data/Seed/asphyxia_data.json` by
`Data/Seed/KfcSeeder.cs`. That JSON is an extract of the asphyxia plugin's
`data/exg.ts`, `data/nbl.ts`, `data/ii.ts`, `data/booth.ts`. The original
`Stella/Data/*.json` files remain for the optional JSON data-provider mode.