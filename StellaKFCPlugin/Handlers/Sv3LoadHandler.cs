using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stella.Abstractions.Plugins;
using StellaKFCPlugin.Data;
using StellaKFCPlugin.EF;
using StellaKFCPlugin.EF.StaticData;
using StellaKFCPlugin.Models;
using StellaKFCPlugin.Util;

namespace StellaKFCPlugin.Handlers;

/// <summary>
/// GRAVITY WARS (sv3) load handler. Ported from asphyxia kfc/handlers/profiles.ts
/// version === 2 || version === 3 block and kfc/templates/load.pug version === 3 block.
/// </summary>
public class Sv3LoadHandler : StellaHandler
{
    [StellaHandler("game_3", "load", typeof(LoadRequest))]
    public async Task<Sv3LoadResponse> LoadGame3() => await LoadSv3Internal(3);

    [StellaHandler("game_3", "load_m", typeof(LoadMRequest))]
    public async Task<Sv3LoadMResponse> LoadMGame3() => await LoadMSv3Internal(3);

    private async Task<Sv3LoadResponse> LoadSv3Internal(int gameVersion)
    {
        var request = Request as LoadRequest;
        if (request is null) return new Sv3LoadResponse { Result = 1 };

        // sv3 uses dataid element for refid (same as v2)
        var refid = request.Refid ?? request.Dataid;
        if (string.IsNullOrEmpty(refid))
        {
            Logger?.LogWarning("sv3 load: refid is null");
            return new Sv3LoadResponse { Result = 1 };
        }

        using var db = new StellaKFCContext();
        var cfg = PluginConfig as StellaKFCPluginConfig ?? new StellaKFCPluginConfig();
        var dVersion = KfcVersion.GetDateCode(Model);

        var profile = await db.SvProfiles.SingleOrDefaultAsync(x => x.RefId == refid && x.Version == gameVersion);

        if (profile is null)
        {
            // Try to migrate from previous version (v2 -> v3)
            var prevProfile = await db.SvProfiles.SingleOrDefaultAsync(x => x.RefId == refid && x.Version == gameVersion - 1);
            if (prevProfile is not null)
            {
                Logger?.LogInformation("sv3 load: migrating from v{PrevVersion} to v{Version}", gameVersion - 1, gameVersion);
                return new Sv3LoadResponse { Result = 2, Name = prevProfile.Name };
            }
            Logger?.LogInformation("sv3 load: no profile for refid {Refid}", refid);
            return new Sv3LoadResponse { Result = 1 };
        }

        // Update datecode if newer
        if (profile.Datecode < dVersion)
        {
            profile.Datecode = dVersion;
            db.SvProfiles.Update(profile);
            await db.SaveChangesAsync();
        }

        // Load skill data
        var skill = await db.SvSkills.SingleOrDefaultAsync(s => s.Profile == profile.Id && s.Version == gameVersion)
            ?? new SvSkill { Base = 0, Level = 0, Name = 0, Type = 0 };

        // Load items
        var items = await db.SvItems.Where(x => x.Profile == profile.Id && x.Version == gameVersion).ToListAsync();

        // Load courses
        var courses = await db.SvCourseRecords.Where(x => x.Profile == profile.Id && x.Version == gameVersion).ToListAsync();

        // Load policy breaks
        var policyBreaks = await db.SvPolicyBreaks.Where(x => x.RefId == refid && x.Version == gameVersion).ToListAsync();

        // Load story progression
        var story = await db.Sv3Stories.Where(x => x.RefId == refid && x.Version == gameVersion).ToListAsync();

        // Load params
        var param = await db.SvParams.Where(x => x.Profile == profile.Id && x.Version == gameVersion && x.Id == 1).FirstOrDefaultAsync()
            ?? new SvParam { Type = 1, Id = 1, Param = "0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0 0" };

        // Build response
        var response = new Sv3LoadResponse
        {
            Result = 0,
            Name = profile.Name,
            Code = profile.Code,
            GamecoinPacket = profile.Packets,
            GamecoinBlock = profile.Blocks,
            SkillLevel = skill.Level,
            SkillNameId = skill.Name,
            HiddenParam = new HiddenParamElement
            {
                Type = "s32",
                Count = param.Param.Split(' ').Length,
                Value = param.Param
            },
            PlayCount = profile.PlayCount,
            DayCount = profile.DayCount,
            TodayCount = profile.TodayCount,
            BlasterEnergy = profile.BlasterEnergy,
            BlasterCount = profile.BlasterCount,
            Last = new Sv3LastElement
            {
                MusicId = profile.LastMusicId,
                MusicType = profile.LastMusicType,
                SortType = profile.SortType,
                NarrowDown = profile.NarrowDown,
                Headphone = profile.Headphone,
                Hispeed = profile.Hispeed,
                AppealId = profile.AppealId,
                CommentId = 0,
                GaugeOption = profile.GaugeOption,
            },
            Item = new Sv3ItemElement
            {
                Infos = items.Select(i => new Sv3ItemInfo
                {
                    Type = i.Type,
                    Id = i.ItemId,
                    Param = i.Param,
                }).ToList(),
            },
            Skill = new Sv3SkillElement
            {
                CourseAll = courses.Any() ? new Sv3CourseAllElement
                {
                    Courses = courses.Select(c => new Sv3CourseElement
                    {
                        Ssnid = c.SeriesId,
                        Crsid = c.CourseId,
                        ClearType = c.Clear,
                        Rate = c.Rate,
                    }).ToList(),
                } : null,
            },
            Pb = new Sv3PbElement
            {
                Infos = policyBreaks.Select(pb => new Sv3PbInfo
                {
                    Id = pb.Id1,
                    Title = GetPolicyBreakTitle(db, pb.Id1, gameVersion),
                    TitleEng = GetPolicyBreakTitleEng(db, pb.Id1, gameVersion),
                    TargetId = GetPolicyBreakTargetId(db, pb.Id1, gameVersion),
                    Exp = pb.Exp,
                    StartDate = GetPolicyBreakStartDate(db, pb.Id1, gameVersion),
                    EndDate = GetPolicyBreakEndDate(db, pb.Id1, gameVersion),
                    Music = new List<Sv3PbMusic>
                    {
                        new Sv3PbMusic
                        {
                            No = 0,
                            Point = GetPolicyBreakPoint(db, pb.Id1, gameVersion),
                            MusicId = GetPolicyBreakMusicId(db, pb.Id1, gameVersion),
                        }
                    },
                }).ToList(),
                Energies = policyBreaks.Select(pb => new Sv3PbEnergyInfo
                {
                    TargetId = GetPolicyBreakTargetId(db, pb.Id1, gameVersion),
                    Energy = pb.Exp,
                }).ToList(),
            },
            Story = story.Any() ? new Sv3StoryElement
            {
                Infos = story.Select(s => new Sv3StoryInfo
                {
                    StoryId = s.StoryId,
                    ProgressId = s.ProgressId,
                    ProgressParam = s.ProgressParam,
                    ClearCnt = s.ClearCnt,
                    RouteFlg = s.RouteFlg,
                }).ToList(),
            } : null,
        };

        // Add creator item if > 1
        if (profile.CreatorItem > 1)
        {
            response.CreatorItem = new Sv3CreatorItemElement
            {
                Info = new Sv3CreatorItemInfo
                {
                    CreatorType = (uint)profile.CreatorItem,
                    ItemId = 0,
                    Param = 0,
                },
            };
        }

        return response;
    }

    // v2->v3 clear lamp remapping (asphyxia gwClearLamp)
    private static readonly int[] GwClearLampOrder = { 0, 1, 2, 4, 5, 3 };

    private async Task<Sv3LoadMResponse> LoadMSv3Internal(int gameVersion)
    {
        var request = Request as LoadMRequest;
        if (request is null) return new Sv3LoadMResponse { Status = "1" };

        // LoadMRequest only has Refid (dataid is normalized by root element renaming)
        var refid = request.Refid;
        if (string.IsNullOrEmpty(refid))
        {
            Logger?.LogWarning("sv3 load_m: refid is null");
            return new Sv3LoadMResponse { Status = "1" };
        }

        using var db = new StellaKFCContext();

        var profile = await db.SvProfiles.SingleOrDefaultAsync(x => x.RefId == refid && x.Version == gameVersion);
        if (profile is null)
        {
            Logger?.LogInformation("sv3 load_m: no profile for refid {Refid}", refid);
            return new Sv3LoadMResponse { Status = "1" };
        }

        // Current version scores (new section)
        var scores = await db.SvScores.Where(x => x.Profile == profile.Id && x.Version == gameVersion).ToListAsync();

        // Previous version scores (old section) — asphyxia merges from version-1
        var prevVersion = gameVersion - 1;
        var oldScores = await db.SvScores.Where(x => x.Profile == profile.Id && x.Version == prevVersion).ToListAsync();

        var response = new Sv3LoadMResponse();

        foreach (var s in scores)
        {
            response.New.Music.Add(new Sv3LoadMMusicNew
            {
                MusicId = (uint)s.MusicId,
                MusicType = (uint)s.Type,
                Score = (uint)s.Score,
                Cnt = (uint)s.PlayCount,
                ClearType = (uint)s.Clear,
                ScoreGrade = (uint)s.Grade,
                BtnRate = (uint)s.ButtonRate,
                LongRate = (uint)s.LongRate,
                VolRate = (uint)s.VolRate,
            });
        }

        foreach (var s in oldScores)
        {
            // Remap clear lamp for old section
            int remappedClear = (s.Clear >= 0 && s.Clear < GwClearLampOrder.Length)
                ? GwClearLampOrder[s.Clear] : s.Clear;
            response.Old.Music.Add(new Sv3LoadMMusicOld
            {
                MusicId = (uint)s.MusicId,
                MusicType = (uint)s.Type,
                Score = (uint)s.Score,
                Cnt = (uint)s.PlayCount,
                ClearType = (uint)remappedClear,
                ScoreGrade = (uint)s.Grade,
            });
        }

        return response;
    }

    // Policy break data helpers — read from sv_static_policy_break table.
    // The seed data (gw_data.json POLICY_BREAK3) populates these at startup.
    private static SvPolicyBreakData? FindPolicyBreakData(StellaKFCContext db, int pbid, int version)
    {
        return db.SvPolicyBreakDatas.FirstOrDefault(p => p.Pbid == pbid && p.Version == version);
    }

    private static string GetPolicyBreakTitle(StellaKFCContext db, int id, int version)
    {
        var data = FindPolicyBreakData(db, id, version);
        return data?.TitleJ ?? $"Policy Break #{id}";
    }

    private static string GetPolicyBreakTitleEng(StellaKFCContext db, int id, int version)
    {
        var data = FindPolicyBreakData(db, id, version);
        return data?.TitleE ?? $"Policy Break #{id}";
    }

    private static int GetPolicyBreakTargetId(StellaKFCContext db, int id, int version)
    {
        var data = FindPolicyBreakData(db, id, version);
        return data?.TargetId ?? 0;
    }

    private static ulong GetPolicyBreakStartDate(StellaKFCContext db, int id, int version)
    {
        var data = FindPolicyBreakData(db, id, version);
        return data != null ? (ulong)data.StartDate : 0;
    }

    private static ulong GetPolicyBreakEndDate(StellaKFCContext db, int id, int version)
    {
        var data = FindPolicyBreakData(db, id, version);
        return data != null ? (ulong)data.EndDate : 0;
    }

    private static int GetPolicyBreakPoint(StellaKFCContext db, int id, int version)
    {
        var data = FindPolicyBreakData(db, id, version);
        return data?.RwrdPoint ?? 24000;
    }

    private static int GetPolicyBreakMusicId(StellaKFCContext db, int id, int version)
    {
        var data = FindPolicyBreakData(db, id, version);
        return data?.RwrdMusicId ?? 0;
    }
}
