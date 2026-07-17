using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stella.Abstractions.Plugins;
using StellaKFCPlugin.Data;
using StellaKFCPlugin.EF;
using StellaKFCPlugin.EF.StaticData;
using StellaKFCPlugin.Models;
using StellaKFCPlugin.Util;
using static StellaKFCPlugin.Util.KfcVersion;

namespace StellaKFCPlugin.Handlers
{
    /// <summary>
    /// Unified <c>common</c> handler serving both EXCEED GEAR (sv6_common) and
    /// NABLA (sv7_common). Ported from asphyxia kfc/handlers/common.ts: emits
    /// events, courses, valgene/apigene, arena, extend, music_limited, music
    /// overrides, weekly_music, and date-based event flags.
    /// </summary>
    public class CommonHandler : StellaHandler
    {
        // Date-based event flag additions (asphyxia common.ts L576-596).
        private static readonly string[] HalloweenDates = { "10/24/", "10/25/", "10/26/", "10/27/", "10/28/", "10/29/", "10/30/", "10/31/" };
        private static readonly string[] ChristmasDates = { "12/24/", "12/25/", "12/26/" };

        [StellaHandler("game", "sv6_common", typeof(GetCommonRequest))]
        public async Task<GetCommonResponse> GetCommon() => await BuildCommon(6);

        [StellaHandler("game", "sv7_common", typeof(GetCommonRequest))]
        public async Task<GetCommonResponse> GetCommonNabla() => await BuildCommon(7);

        [StellaHandler("game", "common", typeof(GetCommonRequest))]
        public async Task<GetCommonResponse> GetCommonBare() => await BuildCommon(Math.Abs(KfcVersion.GetVersion(Model)));

        private async Task<GetCommonResponse> BuildCommon(int gameVersion)
        {
            using var db = new StellaKFCContext();
            try
            {
                var provider = new DbDataProvider(db, gameVersion);
                var date = DateTime.Now;
                var currentYmd = CurrentYmd(date);
                var currentDate = date.ToString("M/d/yyyy", System.Globalization.CultureInfo.InvariantCulture);
                var dVersion = GetDateCode(Model);
                var cabType = GetCabType(Model);

                var events = new List<string>(provider.GetEvents());
                var courses = provider.GetCourses();
                var extend = new List<ExtendInfoRaw>();
                var information = provider.GetInformation();
                var licensedSongs = provider.GetLicensedSongs();
                var valgeneInfo = provider.GetValgeneInfo();
                var valgeneCatalog = provider.GetValgeneCatalog();
                var apigeneInfo = gameVersion == 7 ? provider.GetApigeneInfo() : new List<SvApigeneData>();
                var apigeneCatalog = gameVersion == 7 ? provider.GetApigeneCatalog() : new List<SvApigeneCatalog>();
                var currentArena = provider.GetCurrentArena();
                var arenaItems = provider.GetArenaStationItems();
                var musicOverride = provider.GetMusicOverrides();
                var egSongsLocked = gameVersion == 7 ? provider.GetEgSongsLocked() : Array.Empty<SvEgSongLockedCategory>();
                var kfcConfig = PluginConfig as StellaKFCPluginConfig ?? new StellaKFCPluginConfig();

                // Information notices -> extend type 1 entries (asphyxia common.ts L345-365).
                foreach (var info in information.Where(i => CheckVerStart(dVersion, i.MinVersion, i.StartDate, date)))
                {
                    // asphyxia common.ts L346: parseInt(date.getTime()/100000) * 100
                    long currentTime = KfcVersion.UnixMs(date.ToUniversalTime()) / 100000 * 100;
                    extend.Add(new ExtendInfoRaw
                    {
                        Id = info.InfoId, Type = 1,
                        Params = new object[] { 1, currentTime, 0, 0, 0, "[f:0]SERVER INFORMATION", info.InfoStr, "", "", "" },
                    });
                }

                // Megamix extends (asphyxia megamix1..4 -> extend type 17, id 91..94).
                var megamix = provider.GetMegamix();
                uint extendId = 91;
                foreach (var m in megamix.OrderBy(x => x.MegamixNo))
                {
                    extend.Add(new ExtendInfoRaw
                    {
                        Id = extendId++, Type = 17,
                        Params = new object[] { 0, 0, 0, 0, 0, m.SongIds, "", "", "", "" },
                    });
                }

                // Notification extend (free-software banner).
                {
                    long now = KfcVersion.UnixMs(date.ToUniversalTime());
                    extend.Add(new ExtendInfoRaw
                    {
                        Id = 1, Type = 1,
                        Params = new object[] { 1, KfcVersion.UnixMs(date.ToUniversalTime())/100000, 0, 0, 0, $"[f:0] NOTIFICATION\nFREE SOFTWARE\n{date:s}", "", "", "", "" },
                    });
                }

                var response = new GetCommonResponse { Status = "0" };

                // --- Events ---
                AddDateEvents(events, currentDate, gameVersion, provider);
                response.Event = new EventElement
                {
                    Infos = events.Select(e => new EventInfo { EventId = e }).ToList(),
                };

                // --- Valgene ---
                response.Valgene = BuildValgene(valgeneInfo, valgeneCatalog, dVersion);

                // --- Apigene (NABLA only) ---
                if (gameVersion == 7)
                {
                    response.Apigene = BuildApigene(apigeneInfo, apigeneCatalog, dVersion);
                }

                // --- Arena ---
                response.Arena = BuildArena(currentArena, arenaItems, kfcConfig, dVersion, date);

                // --- Skill courses ---
                response.SkillCourse = BuildSkillCourses(courses, dVersion, cabType);

                // --- Extend ---
                response.Extend = BuildExtend(extend);

                // --- Music override (asphyxia common.ts L319-343) ---
                // Filter by start date (checkVerStart(0,0,m.start,date)); each
                // song emits two sibling <info> elements (info + charts).
                response.Music = new MusicOverrideElement
                {
                    Overrides = musicOverride
                        .Where(m => KfcVersion.CheckVerStart(0, 0, m.StartDate, date))
                        .ToList(),
                };

                // --- Music limited ---
                response.MusicLimited = await BuildMusicLimited(db, provider, gameVersion, dVersion, currentYmd, cabType, licensedSongs, egSongsLocked, kfcConfig);

                // --- Weekly music ---
                response.WeeklyMusic = BuildWeeklyMusic(db, dVersion, date);

                return response;
            }
            catch (Exception ex)
            {
                Logger?.LogError(ex, "CommonHandler.BuildCommon failed");
                throw new Stella.Abstractions.StellaHandlerException(500);
            }
        }

        private void AddDateEvents(List<string> events, string currentDate, int gameVersion, IDataProvider provider)
        {
            if (currentDate.StartsWith("2/5/")) events.Add("EVENTDATE_ONIGO");
            if (currentDate.StartsWith("2/14/")) events.Add("VALENTINES_DAY_2024");
            if (currentDate.StartsWith("2/15/")) events.Add("WHITE_DAY_2024");
            if (currentDate.StartsWith("4/1/"))
            {
                events.Add("EVENTDATE_APRILFOOL");
                events.Add("YUKKURI_RASIS_CREW_ENABLE");
                events.Add("YUKKURI_RASIS_TITLE_ENABLE");
                events.Add("APRIL_RAINBOW_LINE_ENABLE");
            }
            if (currentDate.StartsWith("5/10/")) events.Add("EVENTDATE_GOTT");
            if (HalloweenDates.Any(d => currentDate.StartsWith(d))) events.Add("HALLOWEEN_EVENT");
            if (ChristmasDates.Any(d => currentDate.StartsWith(d))) events.Add("MERRY_CHRISTMAS_2023");
        }

        private ValgeneElement BuildValgene(IReadOnlyList<EF.StaticData.SvValgeneData> info,
            IReadOnlyList<EF.StaticData.SvValgeneCatalog> catalog, int dVersion)
        {
            var el = new ValgeneElement();
            foreach (var v in info.Where(i => dVersion >= i.MinVersion))
            {
                el.Infos.Add(new ValgeneInfo
                {
                    ValgeneName = v.ValgeneName,
                    ValgeneNameEnglish = v.ValgeneNameEnglish,
                    ValgeneId = v.ValgeneId,
                });
            }
            foreach (var c in catalog)
            {
                if (info.All(i => i.ValgeneId != c.ValgeneId)) continue;
                var v = info.First(i => i.ValgeneId == c.ValgeneId);
                if (dVersion < v.MinVersion) continue;
                el.Catalogs.Add(new ValgenesCatalog
                {
                    ValgeneId = c.ValgeneId, ItemType = c.ItemType, ItemId = c.ItemId, Rarity = c.Rarity,
                });
            }
            return el;
        }

        private ApigeneElement BuildApigene(IReadOnlyList<EF.StaticData.SvApigeneData> info,
            IReadOnlyList<EF.StaticData.SvApigeneCatalog> catalog, int dVersion)
        {
            var el = new ApigeneElement();
            foreach (var a in info.Where(i => dVersion >= i.MinVersion))
            {
                el.Infos.Add(new ApigeneInfo
                {
                    ApigeneId = a.ApigeneId, Name = a.Name, NameEnglish = a.NameEnglish,
                    CommonRate = a.CommonRate, UncommonRate = a.UncommonRate, RareRate = a.RareRate,
                    Price = a.Price, NoDuplicate = a.NoDuplicate,
                });
            }
            foreach (var c in catalog)
            {
                if (info.All(i => i.ApigeneId != c.ApigeneId)) continue;
                var a = info.First(i => i.ApigeneId == c.ApigeneId);
                if (dVersion < a.MinVersion) continue;
                el.Catalogs.Add(new ApigeneCatalog
                {
                    ApigeneId = c.ApigeneId, Rarity = c.Rarity, ItemType = c.ItemType, ItemId = c.ItemId,
                });
            }
            return el;
        }

        private ArenaElement BuildArena(SvCurrentArena? currentArena, IReadOnlyList<EF.StaticData.SvArenaStationItem> arenaItems,
            StellaKFCPluginConfig cfg, int dVersion, DateTime date)
        {
            var el = new ArenaElement();
            if (currentArena == null || currentArena.Season == 0) return el;
            bool arenaOpen = cfg.ArenaOpen || (KfcVersion.UnixMs(date.ToUniversalTime()) < currentArena.TimeEnd);
            bool shopOpen = arenaOpen && cfg.ArenaSession != 0;
            if (!arenaOpen || dVersion < 20220425) return el;

            el.Season = currentArena.Season;
            el.Rule = currentArena.Rule;
            el.RankMatchTarget = currentArena.RankMatchTarget;
            el.TimeStart = (ulong)currentArena.TimeStart;
            el.TimeEnd = (ulong)currentArena.TimeEnd;
            el.ShopStart = (ulong)currentArena.ShopStart;
            el.ShopEnd = (ulong)currentArena.ShopEnd;
            el.IsOpen = arenaOpen;
            el.IsShop = shopOpen;

            if (shopOpen && cfg.ArenaStation != null)
            {
                var station = arenaItems.FirstOrDefault(s => s.SetName == cfg.ArenaStation);
                if (station != null && dVersion >= station.MinVersion)
                {
                    var items = JArray.Parse(station.ItemsJson);
                    foreach (var item in items.OfType<JArray>())
                    {
                        if (item.Count < 6) continue;
                        el.Catalogs.Add(new ArenaCatalog
                        {
                            CatalogId = item[0].Value<int>(),
                            CatalogType = item[1].Value<int>(),
                            Price = item[2].Value<int>(),
                            ItemType = item[3].Value<int>(),
                            ItemId = item[4].Value<int>(),
                            Param = item[5].Value<int>(),
                        });
                    }
                }
            }
            return el;
        }

        private SkillCourseElement BuildSkillCourses(IReadOnlyList<EF.StaticData.SvCourseData> courses, int dVersion, string cabType)
        {
            var el = new SkillCourseElement();
            foreach (var s in courses)
            {
                if (dVersion < s.MinVersion) continue;
                var seasonCourses = JArray.Parse(s.CoursesJson);
                foreach (var c in seasonCourses)
                {
                    var ci = new CourseInfo
                    {
                        SeasonId = s.SeriesId,
                        SeasonName = s.SeriesName,
                        SeasonNewFlg = s.IsNew,
                        CourseType = c["type"]!.Value<short>(),
                        CourseId = c["id"]!.Value<short>(),
                        CourseName = c["name"]!.ToString(),
                        SkillLevel = c["level"]!.Value<short>(),
                        SkillType = 0,
                        SkillNameId = c["nameID"]!.Value<short>(),
                        MatchingAssist = c["assist"]?.Value<int>() == 1,
                        ClearRate = 5000,
                        AvgScore = 15000000,
                    };
                    foreach (var t in c["tracks"]!)
                        ci.Tracks.Add(new TrackInfo { TrackNo = t["no"]!.Value<short>(), MusicId = t["mid"]!.Value<int>(), MusicType = (sbyte)t["mty"]!.Value<int>() });
                    el.Infos.Add(ci);
                }

                // God courses for G/H cabinets (asphyxia common.ts L668-690).
                if ((cabType == "G" || cabType == "H") && s.HasGod == 1 && dVersion >= 20230530)
                {
                    foreach (var c in seasonCourses)
                    {
                        var ci = new CourseInfo
                        {
                            SeasonId = s.SeriesId,
                            SeasonName = s.SeriesName,
                            SeasonNewFlg = s.IsNew,
                            CourseType = c["type"]!.Value<short>(),
                            CourseId = c["id"]!.Value<short>(),
                            CourseName = c["name"]!.ToString(),
                            SkillLevel = c["level"]!.Value<short>(),
                            SkillType = s.HasGod,
                            SkillNameId = c["nameID"]!.Value<short>(),
                            MatchingAssist = c["assist"]?.Value<int>() == 1,
                            ClearRate = 5000,
                            AvgScore = 15000000,
                        };
                        foreach (var t in c["tracks"]!)
                            ci.Tracks.Add(new TrackInfo { TrackNo = t["no"]!.Value<short>(), MusicId = t["mid"]!.Value<int>(), MusicType = (sbyte)t["mty"]!.Value<int>() });
                        el.Infos.Add(ci);
                    }
                }
            }
            return el;
        }

        private ExtendElement BuildExtend(List<ExtendInfoRaw> extend)
        {
            var el = new ExtendElement();
            foreach (var e in extend)
            {
                el.Infos.Add(new ExtendInfo
                {
                    ExtendId = (uint)e.Id,
                    ExtendType = (uint)e.Type,
                    ParamNum1 = Convert.ToInt32(e.Params[0]),
                    ParamNum2 = Convert.ToInt32(e.Params[1]),
                    ParamNum3 = Convert.ToInt32(e.Params[2]),
                    ParamNum4 = Convert.ToInt32(e.Params[3]),
                    ParamNum5 = Convert.ToInt32(e.Params[4]),
                    ParamStr1 = e.Params[5]?.ToString() ?? "",
                    ParamStr2 = e.Params[6]?.ToString() ?? "",
                    ParamStr3 = e.Params[7]?.ToString() ?? "",
                    ParamStr4 = e.Params[8]?.ToString() ?? "",
                    ParamStr5 = e.Params[9]?.ToString() ?? "",
                });
            }
            return el;
        }

        private async Task<MusicLimitedElement> BuildMusicLimited(StellaKFCContext db, IDataProvider provider,
            int gameVersion, int dVersion, int currentYmd, string cabType,
            IReadOnlyList<int> licensedSongs, SvEgSongLockedCategory[] egSongsLocked,
            StellaKFCPluginConfig cfg)
        {
            var el = new MusicLimitedElement();
            int songNum = provider.GetSongNum();
            var diffNames = new[] { "novice", "advanced", "exhaust", "infinite", "maximum", "ultimate" };

            if (cfg.UnlockAllSongs)
            {
                int max = await db.SvMusics.AnyAsync() ? await db.SvMusics.MaxAsync(m => m.Id) : songNum;
                for (int i = 1; i <= max; i++)
                    for (byte mt = 0; mt < 6; mt++)
                        el.Infos.Add(new MusicLimitedInfo { MusicId = i, MusicType = mt, Limited = 3 });
                return el;
            }

            // Per-song limited computation (asphyxia common.ts L160-262).
            var musics = await db.SvMusics.ToDictionaryAsync(m => m.Id);
            int lastId = musics.Count > 0 ? musics.Keys.Max() : songNum;
            var egMerge = egSongsLocked.SelectMany(c => c.MusicIds).ToArray();

            for (int i = 0; i <= lastId; i++)
            {
                if (!musics.TryGetValue(i, out var song)) continue;
                int absVersion = gameVersion;

                if (absVersion == 6)
                {
                    int limitedNo = 2;
                    if (licensedSongs.Contains(i)) limitedNo += 1;
                    else if (provider.GetValkyrieSongs().Contains(i) && !Regex.IsMatch(cabType, @"^(G|H)$")) limitedNo -= 1;
                    if (i == 2034) limitedNo = 2;
                    for (byte mt = 0; mt < 6; mt++)
                        el.Infos.Add(new MusicLimitedInfo { MusicId = i, MusicType = mt, Limited = (byte)limitedNo });
                }
                else if (absVersion == 7)
                {
                    // NABLA: songs with version == 7 or in EGSONGS_LOCKED crossresonance.
                    if (egMerge.Contains(i))
                    {
                        int limitedNo = 2;
                        if (licensedSongs.Contains(i)) limitedNo += 1;
                        for (byte mt = 0; mt < 6; mt++)
                            el.Infos.Add(new MusicLimitedInfo { MusicId = i, MusicType = mt, Limited = (byte)limitedNo });
                    }
                }

                // Licensed songs released prior to current version.
                if (song.Version > 0 && song.Version < absVersion && licensedSongs.Contains(i))
                {
                    int limitedNo = 3;
                    for (byte mt = 0; mt < 6; mt++)
                        el.Infos.Add(new MusicLimitedInfo { MusicId = i, MusicType = mt, Limited = (byte)limitedNo });
                }
            }
            return el;
        }

        private List<WeeklyMusicInfo> BuildWeeklyMusic(StellaKFCContext db, int dVersion, DateTime date)
        {
            // asphyxia reads webui/asset/config/weeklymusic.json. Stella uses the
            // sv_static_weekly_music table; returns the active week (if any).
            var list = new List<WeeklyMusicInfo>();
            if (dVersion < 20241210) return list;
            var weeks = db.SvWeeklyMusics.AsEnumerable().Where(w =>
                KfcVersion.UnixMs(date.ToUniversalTime()) > w.Start &&
                KfcVersion.UnixMs(date.ToUniversalTime()) <= w.End).ToList();
            foreach (var w in weeks)
            {
                list.Add(new WeeklyMusicInfo { WeekId = w.WeekId, MusicId = w.MusicId, TimeStart = (ulong)w.Start, TimeEnd = (ulong)w.End });
            }
            return list;
        }

        private class ExtendInfoRaw
        {
            public long Id { get; set; }
            public int Type { get; set; }
            public object[] Params { get; set; } = new object[10];
        }
    }
}