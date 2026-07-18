using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stella.Abstractions.WebUI;
using StellaKFCPlugin.EF;
using StellaKFCPlugin.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StellaKFCPlugin.WebUI;

/// <summary>
/// Registers the KFC WebUI AJAX event handlers (mirrors asphyxia
/// <c>R.WebUIEvent</c> calls in kfc/index.ts). Invoked from
/// <see cref="StellaKFCPlugin.RegisterWebUIEvents"/>.
/// </summary>
public static class KfcWebUIEvents
{
    public static void Register(IWebUIEventRouter router)
    {
        router.Register("seedStatic", _ => RunSeed(false));
        router.Register("loadMusicDb", _ => RunSeed(true));
        router.Register("uploadMusicDb", UploadMusicDb);
        router.Register("saveStartupFlags", SaveStartupFlags);
        router.Register("toggleUnlockEvent", ToggleUnlockEvent);
        router.Register("updateProfile", UpdateProfile);
    }

    private static Task<WebUIResult?> RunSeed(bool music)
    {
        using var db = new StellaKFCContext();
        var logger = Microsoft.Extensions.Logging.LoggerFactory.Create(b => b.AddConsole()).CreateLogger("KfcWebUISeeder");
        var res = music
            ? KfcWebUISeeder.LoadMusicDbAsync(db, logger).GetAwaiter().GetResult()
            : KfcWebUISeeder.SeedStatic(db, logger);
        return Task.FromResult<WebUIResult?>(WebUIResult.JsonFrom(new { ok = res.Ok, message = res.Message }));
    }

    private static async Task<WebUIResult?> UploadMusicDb(JsonElement data)
    {
        var bytes = data.TryGetProperty("base64", out var b) ? b.GetString() : null;
        if (string.IsNullOrEmpty(bytes)) return WebUIResult.JsonFrom(new { ok = false, message = "no data" });
        var ok = await KfcWebUISeeder.SaveUploadedMusicDbAsync(Convert.FromBase64String(bytes));
        if (!ok) return WebUIResult.JsonFrom(new { ok = false, message = "could not write file" });
        using var db = new StellaKFCContext();
        var logger = Microsoft.Extensions.Logging.LoggerFactory.Create(b => b.AddConsole()).CreateLogger("KfcWebUISeeder");
        var res = await KfcWebUISeeder.LoadMusicDbAsync(db, logger);
        return WebUIResult.JsonFrom(new { ok = res.Ok, message = res.Message });
    }

    private static async Task<WebUIResult?> SaveStartupFlags(JsonElement data)
    {
        var cfg = await ReadConfigAsync();
        if (data.TryGetProperty("unlock_all_songs", out var u1)) cfg.UnlockAllSongs = u1.GetBoolean();
        if (data.TryGetProperty("arena_open", out var u2)) cfg.ArenaOpen = u2.GetBoolean();
        if (data.TryGetProperty("arena_no_endtime", out var u3)) cfg.ArenaNoEndtime = u3.GetBoolean();
        if (data.TryGetProperty("arena_session", out var u4)) cfg.ArenaSession = GetInt32(u4);
        if (data.TryGetProperty("arena_station", out var u5)) cfg.ArenaStation = u5.GetString();
        if (data.TryGetProperty("unlock_all_navigators", out var u6)) cfg.UnlockAllNavigators = u6.GetBoolean();
        if (data.TryGetProperty("unlock_all_appeal_cards", out var u7)) cfg.UnlockAllAppealCards = u7.GetBoolean();
        if (data.TryGetProperty("unlock_all_valk_items", out var u8)) cfg.UnlockAllValkItems = u8.GetBoolean();
        if (data.TryGetProperty("use_blasterpass", out var u9)) cfg.UseBlasterPass = u9.GetBoolean();
        if (data.TryGetProperty("gw_mission", out var g1)) cfg.GwMission = g1.GetBoolean();
        if (data.TryGetProperty("gw_gene", out var g2)) cfg.GwGene = g2.GetBoolean();
        await WriteConfigAsync(cfg);

        // Event flag toggles (keys prefixed with "flag_") — asphyxia flags.json.
        using (var db = new StellaKFCContext())
        {
            foreach (var flag in db.SvStartupFlags)
            {
                var key = "flag_" + flag.FlagId;
                flag.Enabled = data.TryGetProperty(key, out var fv) && fv.GetBoolean();
            }
            await db.SaveChangesAsync();
        }
        return WebUIResult.JsonFrom(new { ok = true });
    }

    private static async Task<WebUIResult?> ToggleUnlockEvent(JsonElement data)
    {
        var version = data.TryGetProperty("version", out var v) ? GetInt32(v) : 6;
        var eventId = data.TryGetProperty("event_id", out var e) ? e.GetString() : null;
        if (string.IsNullOrEmpty(eventId)) return WebUIResult.JsonFrom(new { ok = false, message = "missing event_id" });
        using var db = new StellaKFCContext();
        // asphyxia webui/asset/config/events.json [id].toggle: toggles an event
        // in the SvEventList (the events6/7 catalog), NOT the static EVENT6/7
        // flags. CommonHandler.AddEventExtends only emits extends for events
        // whose SvEventList.Enabled is true.
        var ev = db.SvEventLists.FirstOrDefault(x => x.Version == version && x.EventId == eventId);
        if (ev is null) return WebUIResult.JsonFrom(new { ok = false, message = "event not found" });

        // Sub-toggle (object-toggle / prefix gift+cross_online events): flip one
        // sub-key inside SettingsJson.toggle. asphyxia stores these per-song
        // toggles in eventConfig[id].toggle as an object keyed by <eventId>_<idx>.
        if (data.TryGetProperty("subkey", out var sk) && sk.ValueKind == JsonValueKind.String)
        {
            var subkey = sk.GetString();
            if (!string.IsNullOrEmpty(subkey))
            {
                JObject cfg = string.IsNullOrEmpty(ev.SettingsJson) ? new JObject() : JObject.Parse(ev.SettingsJson);
                var toggle = cfg["toggle"] as JObject ?? new JObject();
                bool cur = toggle[subkey]?.Type == JTokenType.Boolean && toggle[subkey]!.Value<bool>();
                toggle[subkey] = !cur;
                cfg["toggle"] = toggle;
                ev.SettingsJson = cfg.ToString(Formatting.None);
                await db.SaveChangesAsync();
                return WebUIResult.JsonFrom(new { ok = true, enabled = !cur, subkey });
            }
        }

        // Boolean toggle (direct events + stamp/tama/variant/achmissions extends).
        ev.Enabled = !ev.Enabled;
        // Optional per-event settings (variant gate minOverTrackRank/minSealDiff,
        // achmissions mission toggles) — stored as JSON in SettingsJson.
        if (data.TryGetProperty("settings", out var s) && s.ValueKind == JsonValueKind.Object)
            ev.SettingsJson = s.GetRawText();
        await db.SaveChangesAsync();
        return WebUIResult.JsonFrom(new { ok = true, enabled = ev.Enabled });
    }

    private static async Task<WebUIResult?> UpdateProfile(JsonElement data)
    {
        var refid = data.TryGetProperty("refid", out var r) ? r.GetString() : null;
        var version = data.TryGetProperty("version", out var vv) ? GetInt32(vv) : 6;
        if (string.IsNullOrEmpty(refid)) return WebUIResult.JsonFrom(new { ok = false, message = "missing refid" });
        using var db = new StellaKFCContext();
        var p = db.SvProfiles.FirstOrDefault(x => x.RefId == refid && x.Version == version);
        if (p is null) return WebUIResult.JsonFrom(new { ok = false, message = "profile not found" });
        if (data.TryGetProperty("name", out var n) && !string.IsNullOrWhiteSpace(n.GetString()))
        {
            var valid = new string(n.GetString()!.ToUpper()
                .Where(c => "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!?#$&*-.".Contains(c)).ToArray());
            if (valid.Length > 0) p.Name = valid.Length > 8 ? valid[..8] : valid;
        }
        if (data.TryGetProperty("appeal_id", out var a)) p.AppealId = (ushort)GetInt32(a);
        await db.SaveChangesAsync();
        return WebUIResult.JsonFrom(new { ok = true });
    }

    // Frontend forms send numbers as strings (input.value), so accept both.
    private static int GetInt32(JsonElement e) => e.ValueKind == JsonValueKind.String
        ? (int.TryParse(e.GetString(), out var s) ? s : 0)
        : (e.TryGetInt32(out var n) ? n : 0);

    // ---- config file persistence (plugin_kfc.json only, no env) ----
    private static string ConfigPath => StellaKFCContext.ResolvePluginConfigPath("plugin_kfc.json");

    private static readonly System.Text.Json.JsonSerializerOptions JsonOpts = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.SnakeCaseLower,
    };

    private static async Task<StellaKFCPluginConfig> ReadConfigAsync()
    {
        var json = await File.ReadAllTextAsync(ConfigPath);
        return System.Text.Json.JsonSerializer.Deserialize<StellaKFCPluginConfig>(json, JsonOpts) ?? new StellaKFCPluginConfig();
    }

    private static async Task WriteConfigAsync(StellaKFCPluginConfig cfg)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(cfg, JsonOpts);
        await File.WriteAllTextAsync(ConfigPath, json);
        // Refresh the live PluginConfig so the running server reflects changes.
        var plugin = Stella.Abstractions.Plugins.StellaPluginRegistry.GetByGameCode("KFC");
        if (plugin is StellaKFCPlugin kfc) kfc.PluginConfig = cfg;
    }
}
