using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Logging;
using Stella.Abstractions.Plugins;
using StellaKFCPlugin.Data;
using StellaKFCPlugin.EF;
using StellaKFCPlugin.Models;
using StellaKFCPlugin.Util;

namespace StellaKFCPlugin.Handlers
{
    /// <summary>
    /// Unified <c>load</c>/<c>load_m</c>/<c>load_r</c> handler serving both
    /// EXCEED GEAR (sv6_*) and NABLA (sv7_*). Ported from asphyxia
    /// kfc/handlers/profiles.ts (<c>load</c>/<c>loadScore</c>/<c>rival</c>) and
    /// templates/load.pug.
    /// </summary>
    public class LoadHandler : StellaHandler
    {
        [StellaHandler("game", "sv6_load", typeof(LoadRequest))]
        public async Task<LoadResponse> Load() => await LoadInternal(6);

        [StellaHandler("game", "sv7_load", typeof(LoadRequest))]
        public async Task<LoadResponse> LoadNabla() => await LoadInternal(7);

        [StellaHandler("game", "load", typeof(LoadRequest))]
        public async Task<LoadResponse> LoadBare() => await LoadInternal(Math.Abs(KfcVersion.GetVersion(Model)));

        [StellaHandler("game", "sv6_load_m", typeof(LoadMRequest))]
        public async Task<LoadMResponse> LoadM() => await LoadMInternal(6);

        [StellaHandler("game", "sv7_load_m", typeof(LoadMRequest))]
        public async Task<LoadMResponse> LoadMNabla() => await LoadMInternal(7);

        [StellaHandler("game", "load_m", typeof(LoadMRequest))]
        public async Task<LoadMResponse> LoadMBare() => await LoadMInternal(Math.Abs(KfcVersion.GetVersion(Model)));

        [StellaHandler("game", "sv6_load_r", typeof(LoadRivalRequest))]
        public async Task<LoadRivalResponse> LoadRival() => await LoadRivalInternal(6);

        [StellaHandler("game", "sv7_load_r", typeof(LoadRivalRequest))]
        public async Task<LoadRivalResponse> LoadRivalNabla() => await LoadRivalInternal(7);

        [StellaHandler("game", "load_r", typeof(LoadRivalRequest))]
        public async Task<LoadRivalResponse> LoadRivalBare() => await LoadRivalInternal(Math.Abs(KfcVersion.GetVersion(Model)));

        [StellaHandler("game_3", "load_r", typeof(LoadRivalRequest))]
        public async Task<LoadRivalResponse> LoadRivalBareGame3() => await LoadRivalInternal(Math.Abs(KfcVersion.GetVersion(Model)));


        private async Task<LoadResponse> LoadInternal(int gameVersion)
        {
            var request = Request as LoadRequest;
            if (request is null) return new LoadResponse { Result = 1 };
            using var db = new StellaKFCContext();
            var cfg = PluginConfig as StellaKFCPluginConfig ?? new StellaKFCPluginConfig();
            var dVersion = KfcVersion.GetDateCode(Model);

            var profile = await db.SvProfiles.SingleOrDefaultAsync(x => x.RefId == request.Refid && x.Version == gameVersion);

            // asphyxia: if profile missing, try previous version (v6 -> v7 migration handled in NewHandler).
            if (profile is null)
            {
                // If a v6 profile exists and we're loading v7, the NewHandler has already
                // migrated; otherwise return result=1.
                Logger?.LogInformation("no profile data for RefId: {Refid}", request.Refid);
                return new LoadResponse { Result = 1 };
            }

            // Update datecode if newer.
            if (profile.Datecode < dVersion)
            {
                profile.Datecode = dVersion;
                db.SvProfiles.Update(profile);
                await db.SaveChangesAsync();
            }

           // result=2 when loading a profile from an older game version.
           byte result = (byte)(gameVersion > profile.Version ? 2 : 0);

            // Grant gift-event presents (asphyxia load L820-907). Must run BEFORE
            // the SvItems query below so newly granted items also appear in the
            // response `item` list (asphyxia re-reads items from DB after granting).
            var grantedPresents = await GrantEventPresents(db, profile.Id, gameVersion, dVersion);

            var skill = await db.SvSkills.SingleOrDefaultAsync(s => s.Profile == profile.Id && s.Version == gameVersion)
                ?? new SvSkill { Base = 0, Level = 0, Name = 0, Type = 0 };

            var items = await db.SvItems.Where(x => x.Profile == profile.Id && x.Version == gameVersion).ToListAsync();
            var param = await db.SvParams.Where(x => x.Profile == profile.Id && x.Version == gameVersion).ToListAsync();
            var courses = await db.SvCourseRecords.Where(x => x.Profile == profile.Id && x.Version == gameVersion).ToListAsync();
            var valgeneTicket = await db.SvValgeneTickets.SingleOrDefaultAsync(x => x.Profile == profile.Id);
            // asphyxia profiles.ts L947-952: arena is looked up by the CURRENT
            // season when arena is open (arena_no_endtime || now < time_end), or
            // season 0 when closed. Filtering by season (not just profile+version)
            // also avoids SingleOrDefaultAsync throwing when a profile has arena
            // rows for multiple seasons.
            var currentArena = await db.SvCurrentArenas.FirstOrDefaultAsync(a => a.Version == gameVersion);
            bool arenaOpen = cfg.ArenaNoEndtime || cfg.ArenaOpen || (KfcVersion.UnixMs(DateTime.UtcNow) < (currentArena?.TimeEnd ?? 0));
            int arenaSeason = arenaOpen && currentArena != null ? currentArena.Season : 0;
            var arena = await db.SvArenas.SingleOrDefaultAsync(a => a.Profile == profile.Id && a.Version == gameVersion && a.Season == arenaSeason);
            var variant = await db.SvVariantPowers.SingleOrDefaultAsync(v => v.Profile == profile.Id && v.Version == gameVersion);

            // Unlock navigators/appeal cards if configured (asphyxia unlockNavigators/unlockAppealCards).
            // Order matches asphyxia load L993-1002: navigators -> appeal cards
            // -> removeStampItems -> unlockAppealParts -> generator power (last).
            if (cfg.UnlockAllNavigators)
            {
                for (int i = 0; i < 300; i++)
                    items.Add(new SvItem { Type = 11, ItemId = (uint)i, Param = 15 });
                items.Add(new SvItem { Type = 4, ItemId = 599, Param = 10 });
            }
            if (cfg.UnlockAllAppealCards)
            {
                for (int i = 0; i < 7000; i++)
                    items.Add(new SvItem { Type = 1, ItemId = (uint)i, Param = 1 });
            }

            // Remove stamp items (type 17): only multiples of 4 survive, id divided by 4.
            var stampFiltered = new List<SvItem>();
            foreach (var it in items)
            {
                if (it.Type == 17 && it.ItemId % 4 != 0) continue;
                var copy = new SvItem { Type = it.Type, ItemId = it.ItemId, Param = it.Param };
                if (copy.Type == 17) copy.ItemId /= 4;
                stampFiltered.Add(copy);
            }
            items = stampFiltered;

            // v7: unlock appeal parts if configured.
            if (gameVersion >= 7 && cfg.UnlockAllValkItems)
            {
                for (int i = 0; i <= 50; i++) items.Add(new SvItem { Type = 23, ItemId = (uint)i, Param = 99 });
                for (int i = 0; i <= 200; i++) items.Add(new SvItem { Type = 24, ItemId = (uint)i, Param = 99 });
            }

            // Make generator power always 100% (asphyxia load L999-1002 — added LAST,
            // after all unlock/stamp/appeal-parts processing, matching asphyxia order).
            for (int i = 0; i < 50; i++)
                items.Add(new SvItem { Type = 7, ItemId = (uint)i, Param = 10 });

            // bplSupport handling: >10 means pro (asphyxia L988-989).
            int bplSupport = profile.BplSupport;
            bool bplPro = bplSupport > 10;
            int bplSupportDisp = bplSupport == 0 ? 0 : bplSupport % 10;

            // currentTime = now + 1 day + 12 hours (asphyxia L978-985) for blaster_pass_limit_date.
            long currentTime = KfcVersion.UnixMs(DateTime.UtcNow) + (36 * 60 * 60 * 1000L);

            int creatorItem = profile.CreatorItem == 0 ? 1 : profile.CreatorItem;

            var response = new LoadResponse
            {
                Result = result,
                Name = profile.Name,
                Code = profile.Code,
                SdvxId = profile.Code,
                GamecoinPacket = profile.Packets,
                GamecoinBlock = profile.Blocks,
                AppealId = profile.AppealId,
                LastMusicId = profile.LastMusicId,
                LastMusicType = profile.LastMusicType,
                SortType = profile.SortType,
                Headphone = profile.Headphone,
                BlasterEnergy = profile.BlasterEnergy,
                Hispeed = profile.Hispeed,
                Lanespeed = profile.Lanespeed,
                GaugeOption = profile.GaugeOption,
                ArsOption = profile.ArsOption,
                NotesOption = profile.NotesOption,
                EarlyLateDisp = profile.EarlyLateDisp,
                DrawAdjust = profile.DrawAdjust,
                EffCLeft = profile.EffCLeft,
                EffCRight = profile.EffCRight,
               NarrowDown = profile.NarrowDown,
                // asphyxia load.pug L85 renders `kac_id(__type="str") #{name}`
                // where `name` is profile.name (via the `...profile` spread in the
                // pug render call). asphyxia does not track a separate KAC id, so
                // kac_id always equals the profile NAME. Match that byte-for-byte
                // (using profile.KacId would diverge whenever the player's name
                // differs from the seeded "VOLTEX" default).
                KacId = profile.Name,
                SkillLevel = skill.Level,
                SkillBaseId = skill.Base,
                SkillNameId = skill.Name,
                SkillType = skill.Type,
                SupportTeamId = (bplSupportDisp > 0 && !bplPro) ? bplSupportDisp : null,
                EaShop = new EaShop
                {
                    PacketBooster = 1,
                    BlasterPassEnable = cfg.UseBlasterPass,
                    BlasterPassLimitDate = (ulong)currentTime,
                },
                Eaappli = new Eaappli { Relation = 1 },
                Cloud = new Cloud { Relation = 1 },
                BlockNo = 0,
                PlayCount = profile.PlayCount,
                DayCount = profile.DayCount,
                TodayCount = profile.TodayCount,
                PlayChain = profile.PlayChain,
                MaxPlayChain = profile.MaxPlayChain,
                WeekCount = profile.WeekCount,
                WeekPlayCount = profile.WeekPlayCount,
                WeekChain = profile.WeekChain,
                MaxWeekChain = profile.MaxWeekChain,
            };

            // Skill courses (asphyxia pug L122-133).
            response.Skill = new Skill();
            foreach (var c in courses)
            {
                response.Skill.Course.Add(new SkillCourse
                {
                    Ssnid = c.SeriesId,
                    Crsid = c.CourseId,
                    St = c.SkillType,
                    Sc = c.Score,
                    Ex = c.Exscore,
                    Ct = (short)c.Clear,
                    Gr = c.Grade,
                    Ar = c.Rate,
                    Cnt = c.Count,
                });
            }

            // Items (asphyxia pug L135-140).
            response.Item = new ItemElement
            {
                Infos = items.Select(x => new ItemInfo { Type = x.Type, Id = x.ItemId, Param = x.Param }).ToList(),
            };

            // Presents (asphyxia pug L165-170) — newly granted gift-event items.
            response.Present = new PresentElement
            {
                Infos = grantedPresents.Select(p => new PresentInfo { Type = p.Type, Id = p.Id, Param = p.Param }).ToList(),
            };

            // Params (asphyxia pug L149-154) + akaname entries (type 6 id 0/1/2).
            response.Param = new ParamElement();
            foreach (var p in param)
            {
                response.Param.Infos.Add(new ParamInfo
                {
                    Type = p.Type,
                    Id = p.ParamId,
                    Param = p.Param.Split(' ').Select(int.Parse).ToList(),
                });
            }
            for (int id = 0; id < 3; id++)
            {
                response.Param.Infos.Add(new ParamInfo { Type = 6, Id = id, Param = new List<int> { profile.Akaname } });
            }

            // Valgene ticket — asphyxia pug `if valgeneTicket` (omit when none).
            if (valgeneTicket is not null)
            {
                response.ValgeneTicket = new ValgeneTicket
                {
                    TicketNum = valgeneTicket.TicketNum,
                    LimitDate = valgeneTicket.LimitDate,
                };
            }

            // Arena (asphyxia pug L182-191).
            if (arena is not null)
            {
                response.Arena = new LoadArenaElement
                {
                    LastPlaySeason = arena.Season,
                    RankPoint = arena.RankPoint,
                    ShopPoint = arena.ShopPoint,
                    UltimateRate = arena.UltimateRate,
                    UltimateRankNum = arena.UltimateRankNum,
                    MegamixRate = arena.MegamixRate,
                    RankPlayCnt = arena.RankCount,
                    UltimatePlayCnt = arena.UltimateCount,
                };
            }

            // Variant gate — asphyxia (profiles.ts L953-964) only initialises a
            // zero-filled variant object when dVersion >= 20250422; before that
            // datecode a missing record means variant_gate is omitted (pug `if variant`).
            if (dVersion >= 20250422 || variant != null)
            {
                var overRadarList = new OverRadarList();
                if (variant != null && !string.IsNullOrEmpty(variant.OverRadar))
                {
                    foreach (var v in variant.OverRadar.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                        overRadarList.Add(int.Parse(v));
                }
                response.VariantGate = new VariantGateElement
                {
                    Power = variant?.Power ?? 0,
                    OverRadar = overRadarList,
                    Element = new VariantElement
                    {
                        Notes = variant?.Notes ?? 0,
                        Peak = variant?.Peak ?? 0,
                        Tsumami = variant?.Tsumami ?? 0,
                        Tricky = variant?.Tricky ?? 0,
                        Onehand = variant?.Onehand ?? 0,
                        Handtrip = variant?.Handtrip ?? 0,
                    },
                };
            }

            // Creator item (asphyxia pug L198-203).
            if (creatorItem > 1)
            {
                response.CreatorItem = new CreatorItemElement
                {
                    Info = new CreatorItemInfo { CreatorType = (uint)creatorItem, ItemId = 0, Param = 0 },
                };
            }

            // Additional info — asphyxia pug always renders this element (pro_team_id
            // is conditional, but the additional_info wrapper is always present).
            response.AdditionalInfo = (bplPro && bplSupportDisp > 0)
                ? new AdditionalInfoElement { ProTeamId = bplSupportDisp.ToString() }
                : new AdditionalInfoElement();

            // Weekly music ranking (asphyxia load L953-968): for the current weekly
            // music, look up the requester's rank across difficulties 0..4.
            response.WeeklyMusic = BuildLoadWeeklyMusic(db, request.Refid, gameVersion, dVersion, DateTime.Now);

            return response;
        }

        // asphyxia load L953-968 + webui.ts getRankListDB: for the active weekly
        // music, query weekly music scores for each difficulty (mtype 0..4), rank
        // by exscore desc, and return the requester's entry per difficulty.
        private List<LoadWeeklyMusic> BuildLoadWeeklyMusic(StellaKFCContext db, string refid, int gameVersion, int dVersion, DateTime date)
        {
            var list = new List<LoadWeeklyMusic>();
            if (dVersion < 20241210) return list;
            var nowMs = KfcVersion.UnixMs(date.ToUniversalTime());
            var week = db.SvWeeklyMusics.AsEnumerable()
                .FirstOrDefault(w => nowMs > w.Start && nowMs <= w.End);
            if (week is null) return list;

            for (int mtype = 0; mtype <= 4; mtype++)
            {
                var ranked = db.SvWeeklyMusicScores.AsEnumerable()
                    .Where(s => s.Version == gameVersion && s.Week == week.WeekId &&
                                s.Mid == week.MusicId && s.Mtype == mtype)
                    .OrderByDescending(s => s.Exscore)
                    .ToList();
                if (ranked.Count == 0) continue;
                int rank = 0;
                for (int i = 0; i < ranked.Count; i++)
                {
                    if (ranked[i].RefId == refid) { rank = i + 1; break; }
                }
                if (rank == 0) continue;
                var entry = ranked[rank - 1];
                list.Add(new LoadWeeklyMusic
                {
                    WeekId = week.WeekId,
                    MusicId = week.MusicId,
                    MusicType = mtype,
                    Exscore = (uint)entry.Exscore,
                    Rank = rank,
                });
            }
            return list;
        }

        private async Task<LoadMResponse> LoadMInternal(int gameVersion)
        {
            var request = Request as LoadMRequest;
            if (request is null) return new LoadMResponse { Status = "1", Music = new MusicElement() };
            using var db = new StellaKFCContext();
            var response = new LoadMResponse();

            var profile = await db.SvProfiles.SingleOrDefaultAsync(x => x.RefId == request.Refid && x.Version == gameVersion);
            if (profile is null)
            {
                Logger?.LogInformation("no profile data for RefId: {Refid}", request.Refid);
                return new LoadMResponse { Status = "1", Music = new MusicElement() };
            }

            var scores = await db.SvScores.Where(x => x.Profile == profile.Id && x.Version == gameVersion).ToListAsync();
            response.Music = new MusicElement();
            foreach (var s in scores)
            {
                var param = new List<uint>
                {
                    (uint)s.MusicId, (uint)s.Type, (uint)s.Score, (uint)s.Exscore,
                    (uint)s.Clear, (uint)s.Grade, 0, 0,
                    (uint)s.ButtonRate, (uint)s.LongRate, (uint)s.VolRate,
                };
                if (gameVersion == 7)
                {
                    // asphyxia loadScore v7: 26 params (mid..volforce + 14 zeros)
                    param.Add((uint)s.Volforce);
                    for (int i = 0; i < 14; i++) param.Add(0);
                }
                else
                {
                    // asphyxia loadScore v6: 21 params (mid..volRate + 10 zeros)
                    for (int i = 0; i < 10; i++) param.Add(0);
                }
                response.Music.Infos.Add(new MusicInfo { Param = param });
            }
            return response;
        }

        private async Task<LoadRivalResponse> LoadRivalInternal(int gameVersion)
        {
            var request = Request as LoadRivalRequest;
            if (request is null) return new LoadRivalResponse { Status = "1" };

            using var db = new StellaKFCContext();
            var dVersion = KfcVersion.GetDateCode(Model);
            var response = new LoadRivalResponse();

            // asphyxia rival: mutual rivals of the requester, excluding self.
            var rivals = await db.SvRivals
                .Where(r => r.RefId == request.Refid && r.Mutual && r.Version == gameVersion)
                .ToListAsync();
            rivals = rivals.Where(r => r.RivalRefId != request.Refid).ToList();

            short no = 0;
            foreach (var r in rivals)
            {
                // asphyxia rival: seq = IDToCode(rival.sdvxID) where sdvxID is the
                // rival profile's in-game id. Stella stores the display code on the
                // profile (SvProfile.Code, already 0000-0000), so prefer the looked-
                // up rival profile's Code; fall back to the stored sdvx id.
                var rivalProfile = await db.SvProfiles
                    .SingleOrDefaultAsync(x => x.RefId == r.RivalRefId && x.Version == gameVersion);
                var entry = new RivalEntry
                {
                    No = no++,
                    Seq = rivalProfile?.Code ?? KfcVersion.IdToCode(r.SdvxId),
                    Name = r.Name ?? string.Empty,
                };

                if (rivalProfile is not null)
                {
                    var rivalScores = await db.SvScores
                        .Where(x => x.Profile == rivalProfile.Id && x.Version == gameVersion)
                        .ToListAsync();
                    foreach (var sc in rivalScores)
                    {
                        var param = dVersion < 20230425
                            ? new List<uint> { (uint)sc.MusicId, (uint)sc.Type, (uint)sc.Score, (uint)sc.Clear, (uint)sc.Grade }
                            : new List<uint> { (uint)sc.MusicId, (uint)sc.Type, (uint)sc.Score, (uint)sc.Exscore, (uint)sc.Clear, (uint)sc.Grade };
                        entry.Music.Add(new RivalMusic { Param = param });
                    }
                }
                response.Rivals.Add(entry);
            }
            return response;
        }

        // asphyxia load L820-907: grant gift-event presents for toggled-on events.
        // gift_crew -> item type 11 param 1; gift_ap -> type 1 param 1;
        // gift / cross_online -> type 0 param 23. Boolean-toggle (direct) events
        // use SvEventList.Enabled + the eventItems[eventId] list; object-toggle
        // (prefix) events use SettingsJson {toggle:{subkey:bool}} and per-subkey
        // version/start arrays (VersionsJson/StartsJson). Items already owned are
        // not re-granted. Also grants the April-Fools yukkuri presents when
        // dVersion >= 20250324 and (the aprilyukkuri flag is on OR it is April 1).
        private static async Task<List<(byte Type, uint Id, uint Param)>> GrantEventPresents(
            StellaKFCContext db, int profileId, int gameVersion, int dVersion)
        {
            var presents = new List<(byte, uint, uint)>();
            var typeIds = new Dictionary<string, (byte Type, uint Param)>
            {
                ["gift_crew"] = (11, 1),
                ["gift_ap"] = (1, 1),
                ["gift"] = (0, 23),
                ["cross_online"] = (0, 23),
            };
            var giftTypes = new HashSet<string> { "gift", "gift_ap", "gift_crew", "cross_online" };
            var date = DateTime.Now;
            var events = await db.SvEventLists.Where(e => e.Version == gameVersion).ToListAsync();
            var eventItems = await db.SvEventItems.Where(e => e.Version == gameVersion)
                .ToDictionaryAsync(e => e.ItemKey, e => e.ItemsJson);

            foreach (var eData in events)
            {
                if (!giftTypes.Contains(eData.Type)) continue;
                if (!typeIds.TryGetValue(eData.Type, out var tp)) continue;
                var (itemType, itemParam) = tp;

                // Object-toggle (prefix) events: SettingsJson carries a toggle object
                // keyed by <eventId>_<idx>. Boolean-toggle (direct) events: Enabled.
                JObject? toggleObj = null;
                if (!string.IsNullOrEmpty(eData.SettingsJson))
                {
                    try { toggleObj = JObject.Parse(eData.SettingsJson)?["toggle"] as JObject; }
                    catch { toggleObj = null; }
                }

                if (toggleObj != null && eData.VersionsJson != null && eData.StartsJson != null)
                {
                    var versions = JArray.Parse(eData.VersionsJson);
                    var starts = JArray.Parse(eData.StartsJson);
                    int idx = 0;
                    foreach (var prop in toggleObj.Properties())
                    {
                        if (idx >= starts.Count) break;
                        if (prop.Value?.Type != JTokenType.Boolean || !prop.Value.Value<bool>()) { idx++; continue; }
                        int ver = versions[idx].Value<int>();
                        int start = starts[idx].Value<int>();
                        if (KfcVersion.CheckVerStart(dVersion, ver, start, date))
                            GrantItems(db, profileId, gameVersion, itemType, itemParam, prop.Name, eventItems, presents);
                        idx++;
                    }
                }
                else if (eData.Enabled && KfcVersion.CheckVerStart(dVersion, eData.MinVersion, eData.StartDate, date))
                {
                    GrantItems(db, profileId, gameVersion, itemType, itemParam, eData.EventId, eventItems, presents);
                }
            }

            // April-Fools yukkuri presents (asphyxia load L888-907).
            if (dVersion >= 20250324)
            {
                bool aprilyukkuri = db.SvStartupFlags.FirstOrDefault(f => f.FlagId == "aprilyukkuri")?.Enabled ?? false;
                bool april1 = date.ToString("M/d/yyyy", System.Globalization.CultureInfo.InvariantCulture).StartsWith("4/1/");
                if (aprilyukkuri || april1)
                {
                    GrantSingle(db, profileId, gameVersion, 1, 5546, 1, presents);
                    GrantSingle(db, profileId, gameVersion, 14, 10244, 1, presents);
                }
            }

            if (presents.Count > 0) await db.SaveChangesAsync();
            return presents;
        }

        private static void GrantItems(StellaKFCContext db, int profileId, int gameVersion,
            byte itemType, uint itemParam, string itemKey,
            Dictionary<string, string> eventItems, List<(byte, uint, uint)> presents)
        {
            if (!eventItems.TryGetValue(itemKey, out var json)) return;
            JArray arr;
            try { arr = JArray.Parse(json); } catch { return; }
            foreach (var tok in arr)
            {
                uint itemId = (uint)tok.Value<int>();
                GrantSingle(db, profileId, gameVersion, itemType, itemId, itemParam, presents);
            }
        }

        private static void GrantSingle(StellaKFCContext db, int profileId, int gameVersion,
            byte itemType, uint itemId, uint itemParam, List<(byte, uint, uint)> presents)
        {
            bool exists = db.SvItems.Any(x => x.Profile == profileId && x.Version == gameVersion
                && x.Type == itemType && x.ItemId == itemId);
            if (exists) return;
            db.SvItems.Add(new SvItem { Profile = profileId, Version = gameVersion, Type = itemType, ItemId = itemId, Param = itemParam });
            presents.Add((itemType, itemId, itemParam));
        }
    }
}
