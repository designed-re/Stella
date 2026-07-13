using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

        [StellaHandler("game", "sv6_load_m", typeof(LoadMRequest))]
        public async Task<LoadMResponse> LoadM() => await LoadMInternal(6);

        [StellaHandler("game", "sv7_load_m", typeof(LoadMRequest))]
        public async Task<LoadMResponse> LoadMNabla() => await LoadMInternal(7);

        [StellaHandler("game", "sv6_load_r", typeof(LoadRivalRequest))]
        public async Task<LoadRivalResponse> LoadRival() => new();

        [StellaHandler("game", "sv7_load_r", typeof(LoadRivalRequest))]
        public async Task<LoadRivalResponse> LoadRivalNabla() => new();

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

            var skill = await db.SvSkills.SingleOrDefaultAsync(s => s.Profile == profile.Id && s.Version == gameVersion)
                ?? new SvSkill { Base = 0, Level = 0, Name = 0, Type = 0 };

            var items = await db.SvItems.Where(x => x.Profile == profile.Id && x.Version == gameVersion).ToListAsync();
            var param = await db.SvParams.Where(x => x.Profile == profile.Id && x.Version == gameVersion).ToListAsync();
            var courses = await db.SvCourseRecords.Where(x => x.Profile == profile.Id && x.Version == gameVersion).ToListAsync();
            var valgeneTicket = await db.SvValgeneTickets.SingleOrDefaultAsync(x => x.Profile == profile.Id);
            var arena = await db.SvArenas.SingleOrDefaultAsync(a => a.Profile == profile.Id && a.Version == gameVersion);
            var variant = await db.SvVariantPowers.SingleOrDefaultAsync(v => v.Profile == profile.Id && v.Version == gameVersion);

            // Make generator power always 100% (asphyxia load L999-1002).
            for (int i = 0; i < 50; i++)
                items.Add(new SvItem { Type = 7, ItemId = (uint)i, Param = 10 });

            // Unlock navigators/appeal cards if configured (asphyxia unlockNavigators/unlockAppealCards).
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
                GamecoinPacket = 10000,
                GamecoinBlock = (uint)profile.Pcb,
                AppealId = profile.AppealId,
                LastMusicId = profile.LastMusicId,
                LastMusicType = profile.LastMusicType,
                SortType = profile.SortType,
                Headphone = profile.Headphone,
                BlasterEnergy = profile.BlasterEnergy,
                BlasterCount = profile.BlasterCount,
                ExtrackEnergy = profile.ExtrackEnergy,
                Hispeed = profile.Hispeed,
                Lanespeed = profile.Lanespeed,
                GaugeOption = profile.GaugeOption,
                ArsOption = profile.ArsOption,
                NotesOption = profile.NotesOption,
                EarlyLateDisp = profile.EarlyLateDisp,
                DrawAdjust = profile.DrawAdjust,
                EffCLeft = profile.EffCLeft,
                EffCRight = profile.EffCRight,
                NarrowDown = 0,
                KacId = profile.KacId,
                SkillLevel = skill.Level,
                SkillBaseId = skill.Base,
                SkillNameId = skill.Name,
                SkillType = skill.Type,
                SupportTeamId = (bplSupportDisp > 0 && !bplPro) ? bplSupportDisp : 0,
                EaShop = new EaShop
                {
                    PacketBooster = 1,
                    BlasterPassEnable = cfg.UseBlasterPass,
                    BlasterPassLimitDate = (ulong)currentTime,
                },
                Eaappli = new Eaappli { Relation = 1 },
                Cloud = new Cloud { Relation = 1 },
                BlockNo = profile.Pcb,
                PlayCount = profile.PlayCount,
                DayCount = profile.DayCount,
                TodayCount = profile.TodayCount,
                PlayChain = profile.PlayChain,
                MaxPlayChain = profile.MaxPlayChain,
                WeekCount = profile.WeekCount,
                WeekPlayCount = profile.WeekPlayCount,
                WeekChain = profile.WeekChain,
                MaxWeekChain = profile.MaxWeekChain,
                ValgeneTicket = new ValgeneTicket(),
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

            // Valgene ticket.
            if (valgeneTicket is not null)
            {
                response.ValgeneTicket.TicketNum = valgeneTicket.TicketNum;
                response.ValgeneTicket.LimitDate = valgeneTicket.LimitDate;
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

            // Variant gate (asphyxia pug L205-214).
            if (variant is not null)
            {
                var overRadar = string.IsNullOrEmpty(variant.OverRadar)
                    ? new List<int>()
                    : variant.OverRadar.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
                response.VariantGate = new VariantGateElement
                {
                    Power = variant.Power,
                    OverRadar = string.Join(" ", overRadar),
                    Element = new VariantElement
                    {
                        Notes = variant.Notes, Peak = variant.Peak, Tsumami = variant.Tsumami,
                        Tricky = variant.Tricky, Onehand = variant.Onehand, Handtrip = variant.Handtrip,
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

            // Additional info (pro_team_id when bplPro && bplSupport > 0).
            if (bplPro && bplSupport > 0)
            {
                response.AdditionalInfo = new AdditionalInfoElement { ProTeamId = bplSupport.ToString() };
            }

            return response;
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
                    param.Add((uint)s.Volforce);
                    for (int i = 0; i < 16; i++) param.Add(0);
                }
                else
                {
                    for (int i = 0; i < 10; i++) param.Add(0);
                }
                response.Music.Infos.Add(new MusicInfo { Param = param });
            }
            return response;
        }
    }
}