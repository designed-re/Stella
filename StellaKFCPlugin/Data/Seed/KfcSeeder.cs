using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StellaKFCPlugin.EF;
using StellaKFCPlugin.EF.StaticData;

namespace StellaKFCPlugin.Data.Seed;

/// <summary>
/// Seeds the sv_static_* tables from <c>asphyxia_data.json</c> (an extract of
/// asphyxia's data/exg.ts, data/nbl.ts, data/ii.ts, data/booth.ts). Idempotent:
/// only inserts rows that do not yet exist (matched by natural key).
/// Mirrors asphyxia plugin's data modules so handlers consume identical data.
/// </summary>
public static class KfcSeeder
{
    public static void Seed(StellaKFCContext db)
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "Data", "Seed", "asphyxia_data.json");
        if (!File.Exists(path))
            path = Path.Combine(AppContext.BaseDirectory, "Data", "Seed", "asphyxia_data.json");
        if (!File.Exists(path))
        {
            Console.WriteLine("[KfcSeeder] asphyxia_data.json not found; skipping seed.");
            return;
        }

        var root = JObject.Parse(File.ReadAllText(path));
        // The seed JSON is flat (all consts at top level) — it mirrors asphyxia's
        // data/exg.ts, data/nbl.ts, data/ii.ts, data/booth.ts merged into one object.
        var exg = root;
        var nbl = root;
        var ii = root;
        var booth = root;

        SeedEvents(db, 6, exg?["EVENT6"] as JArray);
        SeedEvents(db, 7, nbl?["EVENT7"] as JArray);

        SeedLicensedSongs(db, 6, exg?["LICENSED_SONGS6"] as JArray);
        SeedLicensedSongs(db, 7, nbl?["LICENSED_SONGS7"] as JArray);

        SeedValkyrieSongs(db, 6, exg?["VALKYRIE_SONGS"] as JArray);
        SeedAprilFoolsSongs(db, 6, exg?["APRILFOOLSSONGS"] as JArray);

        SeedCourses(db, 6, exg?["COURSES6"] as JArray);
        SeedCourses(db, 7, nbl?["COURSES7"] as JArray);

        SeedValgene(db, 6, exg?["VALGENE"] as JObject);
        SeedValgene(db, 7, nbl?["VALGENE7"] as JObject);

        SeedApigene(db, 7, nbl?["APIGENE7"] as JObject);

        SeedInformation(db, 6, exg?["INFORMATION6"] as JArray);
        SeedInformation(db, 7, nbl?["INFORMATION7"] as JArray);

        SeedExtends(db, 6, exg?["EXTENDS6"] as JArray);
        SeedExtends(db, 7, nbl?["EXTENDS7"] as JArray);

        SeedMegamix(db, 6, exg);
        SeedCurrentArena(db, 6, exg?["CURRENT_ARENA"] as JObject);
        SeedCurrentArena(db, 7, nbl?["CURRENT_ARENA7"] as JObject);

        SeedArenaStation(db, 6, exg?["ARENA_STATION_ITEMS"] as JObject);
        SeedArenaStation(db, 7, nbl?["ARENA_STATION_ITEMS7"] as JObject);

        SeedMusicOverride(db, 6, exg?["MUSIC_OVERRIDE6"] as JArray);
        SeedMusicOverride(db, 7, nbl?["MUSIC_OVERRIDE7"] as JArray);

        SeedEgSongsLocked(db, 7, nbl?["EGSONGS_LOCKED"] as JObject);

        SeedPolicyBreakData(db, 2, ii?["POLICY_BREAK2"] as JArray);
        SeedHaveNotes(db, ii?["HAVE_NOTE"] as JArray);

        // Unlock events (rich nested data) — serialize per-event as JSON.
        SeedUnlockEvents(db, 6, exg?["UNLOCK_EVENTS6"] as JObject);
        SeedUnlockEvents(db, 7, nbl?["UNLOCK_EVENTS7"] as JObject);

        db.SaveChanges();
        Console.WriteLine("[KfcSeeder] Seed complete.");
    }

    private static void SeedEvents(StellaKFCContext db, int version, JArray? arr)
    {
        if (arr == null) return;
        var existing = db.SvEventDatas.Where(e => e.Version == version).Select(e => e.EventId).ToHashSet();
        int order = 0;
        foreach (var tok in arr)
        {
            var ev = tok!.ToString();
            if (existing.Contains(ev)) { order++; continue; }
            db.SvEventDatas.Add(new SvEventData { Version = version, SortOrder = order, EventId = ev });
            order++;
        }
    }

    private static void SeedLicensedSongs(StellaKFCContext db, int version, JArray? arr)
    {
        if (arr == null) return;
        var existing = db.SvLicensedSongs.Where(l => l.Version == version).Select(l => l.MusicId).ToHashSet();
        foreach (var tok in arr)
        {
            int mid = tok!.Value<int>();
            if (existing.Add(mid))
                db.SvLicensedSongs.Add(new SvLicensedSong { Version = version, MusicId = mid });
        }
    }

    private static void SeedValkyrieSongs(StellaKFCContext db, int version, JArray? arr)
    {
        if (arr == null) return;
        var existing = db.SvValkyrieSongs.Where(v => v.Version == version).Select(v => v.MusicId).ToHashSet();
        foreach (var tok in arr)
        {
            int mid = tok!.Value<int>();
            if (existing.Add(mid))
                db.SvValkyrieSongs.Add(new SvValkyrieSong { Version = version, MusicId = mid });
        }
    }

    private static void SeedAprilFoolsSongs(StellaKFCContext db, int version, JArray? arr)
    {
        if (arr == null) return;
        var existing = db.SvAprilFoolsSongs.Where(a => a.Version == version).Select(a => a.MusicId).ToHashSet();
        foreach (var tok in arr)
        {
            int mid = tok!.Value<int>();
            if (existing.Add(mid))
                db.SvAprilFoolsSongs.Add(new SvAprilFoolsSong { Version = version, MusicId = mid });
        }
    }

    private static void SeedCourses(StellaKFCContext db, int version, JArray? arr)
    {
        if (arr == null) return;
        var existing = db.SvCourseDatas.Where(c => c.Version == version).Select(c => c.SeriesId).ToHashSet();
        foreach (var season in arr!)
        {
            int sid = season["id"]!.Value<int>();
            if (existing.Contains(sid)) continue;
            db.SvCourseDatas.Add(new SvCourseData
            {
                Version = version,
                SeriesId = sid,
                SeriesName = season["name"]!.ToString(),
                IsNew = season["isNew"]?.Value<int>() == 1,
                HasGod = (short)(season["hasGod"]?.Value<int>() ?? 0),
                MinVersion = season["version"]?.Value<int>() ?? 0,
                CoursesJson = season["courses"]!.ToString(Formatting.None),
            });
        }
    }

    private static void SeedValgene(StellaKFCContext db, int version, JObject? val)
    {
        if (val == null) return;
        var infoExisting = db.SvValgeneDatas.Where(v => v.Version == version).Select(v => v.ValgeneId).ToHashSet();
        foreach (var info in val["info"]!)
        {
            int vid = info["valgene_id"]!.Value<int>();
            if (infoExisting.Contains(vid)) continue;
            db.SvValgeneDatas.Add(new SvValgeneData
            {
                Version = version,
                ValgeneId = vid,
                ValgeneName = info["valgene_name"]!.ToString(),
                ValgeneNameEnglish = info["valgene_name_english"]?.ToString() ?? string.Empty,
                MinVersion = info["version"]?.Value<int>() ?? 0,
            });
        }

        var rarity = val["rarity"] as JObject;
        var catExisting = db.SvValgeneCatalogs.Where(c => c.Version == version)
            .Select(c => new { c.ValgeneId, c.ItemType, c.ItemId }).ToHashSet();
        foreach (var cat in val["catalog"]!)
        {
            int volume = cat["volume"]!.Value<int>();
            foreach (var item in cat["items"]!)
            {
                int type = item["type"]!.Value<int>();
                int rarityVal = rarity != null && rarity[type.ToString()] != null
                    ? rarity[type.ToString()]!.Value<int>() : 0;
                foreach (var id in item["item_ids"]!)
                {
                    int itemId = id.Value<int>();
                    var key = new { ValgeneId = volume, ItemType = type, ItemId = itemId };
                    if (catExisting.Contains(key)) continue;
                    db.SvValgeneCatalogs.Add(new SvValgeneCatalog
                    {
                        Version = version, ValgeneId = volume, ItemType = type, ItemId = itemId, Rarity = rarityVal,
                    });
                }
            }
        }
    }

    private static void SeedApigene(StellaKFCContext db, int version, JObject? val)
    {
        if (val == null) return;
        var infoExisting = db.SvApigeneDatas.Where(a => a.Version == version).Select(a => a.ApigeneId).ToHashSet();
        foreach (var info in val["info"]!)
        {
            int aid = info["apigene_id"]!.Value<int>();
            if (infoExisting.Contains(aid)) continue;
            db.SvApigeneDatas.Add(new SvApigeneData
            {
                Version = version,
                ApigeneId = aid,
                Name = info["name"]!.ToString(),
                NameEnglish = info["name_english"]?.ToString() ?? string.Empty,
                CommonRate = info["common_rate"]?.Value<int>() ?? 0,
                UncommonRate = info["uncommon_rate"]?.Value<int>() ?? 0,
                RareRate = info["rare_rate"]?.Value<int>() ?? 0,
                Price = info["price"]?.Value<int>() ?? 0,
                NoDuplicate = info["no_duplicate"]?.Value<bool>() ?? false,
                MinVersion = info["version"]?.Value<int>() ?? 0,
            });
        }

        var rarity = val["rarity"] as JObject;
        var catExisting = db.SvApigeneCatalogs.Where(c => c.Version == version)
            .Select(c => new { c.ApigeneId, c.ItemType, c.ItemId }).ToHashSet();
        foreach (var cat in val["catalog"]!)
        {
            int volume = cat["volume"]!.Value<int>();
            foreach (var item in cat["items"]!)
            {
                int type = item["type"]!.Value<int>();
                int rarityVal = rarity != null && rarity[type.ToString()] != null
                    ? rarity[type.ToString()]!.Value<int>() : 0;
                foreach (var id in item["item_ids"]!)
                {
                    int itemId = id.Value<int>();
                    var key = new { ApigeneId = volume, ItemType = type, ItemId = itemId };
                    if (catExisting.Contains(key)) continue;
                    db.SvApigeneCatalogs.Add(new SvApigeneCatalog
                    {
                        Version = version, ApigeneId = volume, ItemType = type, ItemId = itemId, Rarity = rarityVal,
                    });
                }
            }
        }
    }

    private static void SeedInformation(StellaKFCContext db, int version, JArray? arr)
    {
        if (arr == null) return;
        var existing = db.SvInformationDatas.Where(i => i.Version == version).Select(i => i.InfoId).ToHashSet();
        foreach (var info in arr!)
        {
            int id = info["id"]!.Value<int>();
            if (existing.Contains(id)) continue;
            db.SvInformationDatas.Add(new SvInformationData
            {
                Version = version,
                InfoId = id,
                InfoStr = info["str"]!.ToString(),
                MinVersion = info["version"]?.Value<int>() ?? 0,
                StartDate = info["start"]?.Value<int>() ?? 0,
            });
        }
    }

    private static void SeedExtends(StellaKFCContext db, int version, JArray? arr)
    {
        if (arr == null) return;
        var existing = db.SvExtendDatas.Where(e => e.Version == version).Select(e => e.ExtendId).ToHashSet();
        foreach (var ext in arr!)
        {
            uint eid = ext["id"]!.Value<uint>();
            if (existing.Contains(eid)) continue;
            var p = ext["params"] as JArray;
            db.SvExtendDatas.Add(new SvExtendData
            {
                Version = version,
                ExtendId = eid,
                ExtendType = ext["type"]!.Value<uint>(),
                ParamNum1 = p?[0]?.Value<int>() ?? 0,
                ParamNum2 = p?[1]?.Value<int>() ?? 0,
                ParamNum3 = p?[2]?.Value<int>() ?? 0,
                ParamNum4 = p?[3]?.Value<int>() ?? 0,
                ParamNum5 = p?[4]?.Value<int>() ?? 0,
                ParamStr1 = p?[5]?.ToString() ?? string.Empty,
                ParamStr2 = p?[6]?.ToString() ?? string.Empty,
                ParamStr3 = p?[7]?.ToString() ?? string.Empty,
                ParamStr4 = p?[8]?.ToString() ?? string.Empty,
                ParamStr5 = p?[9]?.ToString() ?? string.Empty,
                MinVersion = ext["version"]?.Value<int>() ?? 0,
                StartDate = ext["start"]?.Value<int>() ?? 0,
            });
        }
    }

    private static void SeedMegamix(StellaKFCContext db, int version, JObject? exg)
    {
        if (exg == null) return;
        var existing = db.SvMegamixDatas.Where(m => m.Version == version).Select(m => m.MegamixNo).ToHashSet();
        for (int i = 1; i <= 4; i++)
        {
            var arr = exg[$"MEGAMIX_SONGS{(i == 1 ? "" : "_" + i)}"] as JArray;
            if (arr == null) continue;
            if (existing.Contains(i)) continue;
            db.SvMegamixDatas.Add(new SvMegamixData
            {
                Version = version, MegamixNo = i,
                SongIds = string.Join(",", arr.Select(t => t.Value<int>())),
            });
        }
    }

    private static void SeedCurrentArena(StellaKFCContext db, int version, JObject? obj)
    {
        if (obj == null) return;
        if (db.SvCurrentArenas.Any(a => a.Version == version)) return;
        db.SvCurrentArenas.Add(new SvCurrentArena
        {
            Version = version,
            Season = obj["season"]!.Value<int>(),
            Rule = obj["rule"]!.Value<short>(),
            RankMatchTarget = obj["rank_match_target"]!.Value<short>(),
            TimeStart = ParseEpochMs(obj["time_start"]),
            TimeEnd = ParseEpochMs(obj["time_end"]),
            ShopStart = ParseEpochMs(obj["shop_start"]),
            ShopEnd = ParseEpochMs(obj["shop_end"]),
        });
    }

    private static long ParseEpochMs(JToken? tok)
    {
        if (tok == null) return 0;
        // asphyxia stores Date.getTime() ms as number; our extract used Number(BigInt(...)) on Date.parse() ms.
        return tok.Type == JTokenType.Integer ? tok.Value<long>() : (long)tok.Value<double>();
    }

    private static void SeedArenaStation(StellaKFCContext db, int version, JObject? obj)
    {
        if (obj == null) return;
        var existing = db.SvArenaStationItems.Where(a => a.Version == version).Select(a => a.SetName).ToHashSet();
        foreach (var prop in obj.Properties())
        {
            if (existing.Contains(prop.Name)) continue;
            var items = prop.Value["items"];
            db.SvArenaStationItems.Add(new SvArenaStationItem
            {
                Version = version,
                SetName = prop.Name,
                MinVersion = prop.Value["version"]?.Value<int>() ?? 0,
                ItemsJson = items?.ToString(Formatting.None) ?? "[]",
            });
        }
    }

    private static void SeedMusicOverride(StellaKFCContext db, int version, JArray? arr)
    {
        if (arr == null) return;
        var existing = db.SvMusicOverrides.Where(m => m.Version == version).Select(m => m.MusicId).ToHashSet();
        foreach (var m in arr!)
        {
            // asphyxia MUSIC_OVERRIDE entries use `music_id` (not `id`); the
            // info keys are every top-level key except `charts` and `start`.
            int mid = m["music_id"]?.Value<int>() ?? 0;
            if (mid == 0 || existing.Contains(mid)) continue;

            var infoObj = new JObject();
            foreach (var prop in m.Children<JProperty>())
            {
                if (prop.Name == "charts" || prop.Name == "start") continue;
                infoObj[prop.Name] = prop.Value.DeepClone();
            }

            db.SvMusicOverrides.Add(new SvMusicOverride
            {
                Version = version,
                MusicId = mid,
                StartDate = m["start"]?.Value<int>() ?? 0,
                InfoJson = infoObj.ToString(Formatting.None),
                ChartsJson = (m["charts"] ?? new JObject()).ToString(Formatting.None),
            });
        }
    }

    private static void SeedEgSongsLocked(StellaKFCContext db, int version, JObject? obj)
    {
        if (obj == null) return;
        var existing = db.SvEgSongLockeds.Where(e => e.Version == version)
            .Select(e => new { e.Category, e.MusicId }).ToHashSet();
        foreach (var prop in obj.Properties())
        {
            foreach (var id in prop.Value!)
            {
                int mid = id.Value<int>();
                var key = new { Category = prop.Name, MusicId = mid };
                if (existing.Contains(key)) continue;
                db.SvEgSongLockeds.Add(new SvEgSongLocked { Version = version, Category = prop.Name, MusicId = mid });
            }
        }
    }

    private static void SeedPolicyBreakData(StellaKFCContext db, int version, JArray? arr)
    {
        if (arr == null) return;
        var existing = db.SvPolicyBreakDatas.Where(p => p.Version == version).Select(p => p.Pbid).ToHashSet();
        foreach (var pb in arr!)
        {
            int id = pb["id"]!.Value<int>();
            if (existing.Contains(id)) continue;
            var rwrd = pb["rwrd"]!;
            db.SvPolicyBreakDatas.Add(new SvPolicyBreakData
            {
                Version = version,
                Pbid = id,
                RwrdType = rwrd["type"]!.Value<int>(),
                RwrdId = rwrd["id"]!.Value<int>(),
                RwrdParam = rwrd["param"]!.Value<int>(),
                StartDate = pb["start"]?.Value<long>() ?? 0,
                EndDate = pb["end"]?.Value<long>() ?? 0,
            });
        }
    }

    private static void SeedHaveNotes(StellaKFCContext db, JArray? arr)
    {
        if (arr == null) return;
        var existing = db.SvHaveNotes.Select(n => new { n.NoteId, n.Param }).ToHashSet();
        foreach (var n in arr!)
        {
            int noteId = n["id"]!.Value<int>();
            int param = n["param"]!.Value<int>();
            if (existing.Contains(new { NoteId = noteId, Param = param })) continue;
            db.SvHaveNotes.Add(new SvHaveNote { NoteId = noteId, Param = param });
        }
    }

    private static void SeedUnlockEvents(StellaKFCContext db, int version, JObject? obj)
    {
        if (obj == null) return;
        var existing = db.SvUnlockEventDatas.Where(e => e.Version == version).Select(e => e.EventId).ToHashSet();
        foreach (var prop in obj.Properties())
        {
            if (prop.Name == "refillStamps") continue;
            if (existing.Contains(prop.Name)) continue;
            var v = prop.Value as JObject;
            string type = v?.Property("type")?.Value?.ToString() ?? "unknown";
            db.SvUnlockEventDatas.Add(new SvUnlockEventData
            {
                Version = version,
                EventId = prop.Name,
                Type = type,
                MinVersion = v?.Property("version")?.Value?.Value<int>() ?? 0,
                StartDate = v?.Property("start")?.Value?.Value<int>() ?? 0,
                DataJson = v?.ToString(Formatting.None) ?? string.Empty,
            });
        }
    }
}