using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stella.Abstractions.Plugins;
using StellaKFCPlugin.EF;
using StellaKFCPlugin.Models;
using StellaKFCPlugin.Util;

namespace StellaKFCPlugin.Handlers;

/// <summary>
/// GRAVITY WARS (sv3) save handler. Ported from asphyxia kfc/handlers/profiles.ts
/// version === 2 || version === 3 blocks.
/// </summary>
public class Sv3SaveHandler : StellaHandler
{
    [StellaHandler("game_3", "save", typeof(Sv3SaveRequest))]
    public async Task<SaveResponse> SaveGame3() => await SaveSv3Internal(3);

    [StellaHandler("game_3", "save_m", typeof(Sv3SaveMRequest))]
    public async Task<SaveMResponse> SaveMGame3() => await SaveMSv3Internal(3);

    [StellaHandler("game_3", "save_c", typeof(SaveCourseRequest))]
    public async Task<SaveResponse> SaveCourseGame3() => await SaveCourseSv3Internal(3);

    private async Task<SaveResponse> SaveSv3Internal(int gameVersion)
    {
        var request = Request as Sv3SaveRequest;
        if (request is null) return new SaveResponse { Status = "1" };

        var refid = request.RefId ?? request.DataId;
        if (string.IsNullOrEmpty(refid)) return new SaveResponse { Status = "1" };

        using var db = new StellaKFCContext();

        var profile = await db.SvProfiles.SingleOrDefaultAsync(x => x.RefId == refid && x.Version == gameVersion);
        if (profile is null) return new SaveResponse { Status = "1" };

        // Update profile data
        profile.Name = request.Name;
        profile.Code = request.Code;
        profile.Packets = request.GamecoinPacket;
        profile.Blocks = request.GamecoinBlock;
        profile.AppealId = request.AppealId;
        profile.LastMusicId = request.MusicId;
        profile.LastMusicType = request.MusicType;
        profile.SortType = request.SortType;
        profile.Headphone = request.Headphone;
        profile.Hispeed = request.Hispeed;
        profile.Lanespeed = request.Lanespeed;
        profile.GaugeOption = request.GaugeOption;
        profile.ArsOption = request.ArsOption;
        profile.NotesOption = request.NotesOption;
        profile.EarlyLateDisp = request.EarlyLateDisp;
        profile.DrawAdjust = request.DrawAdjust;
        profile.EffCLeft = request.EffCLeft;
        profile.EffCRight = request.EffCRight;
        profile.NarrowDown = request.NarrowDown;
        profile.BlasterEnergy += request.EarnedBlasterEnergy;
        profile.Packets += request.EarnedGamecoinPacket;
        profile.Blocks += request.EarnedGamecoinBlock;

        // Update skill data
        var skill = await db.SvSkills.SingleOrDefaultAsync(s => s.Profile == profile.Id && s.Version == gameVersion);
        if (skill is null)
        {
            skill = new SvSkill { Profile = profile.Id, Version = gameVersion };
            db.SvSkills.Add(skill);
        }
        skill.Level = request.SkillLevel;
        skill.Name = request.SkillNameId;

        // Save items
        if (request.Item?.Infos != null)
        {
            foreach (var item in request.Item.Infos)
            {
                var existing = await db.SvItems.SingleOrDefaultAsync(i =>
                    i.Profile == profile.Id && i.Type == item.Type && i.ItemId == item.Id && i.Version == gameVersion);
                if (existing is null)
                {
                    db.SvItems.Add(new SvItem
                    {
                        Profile = profile.Id,
                        Type = item.Type,
                        ItemId = item.Id,
                        Param = item.Param,
                        Version = gameVersion,
                    });
                }
                else
                {
                    existing.Param = item.Param;
                }
            }
        }

        // Save params
        if (request.Param?.Infos != null)
        {
            foreach (var param in request.Param.Infos)
            {
                var existing = await db.SvParams.SingleOrDefaultAsync(p =>
                    p.Profile == profile.Id && p.Type == param.Type && p.Id == param.Id && p.Version == gameVersion);
                if (existing is null)
                {
                    db.SvParams.Add(new SvParam
                    {
                        Profile = profile.Id,
                        Type = param.Type,
                        Id = param.Id,
                        Param = string.Join(" ", param.Param),
                        Version = gameVersion,
                    });
                }
                else
                {
                    existing.Param = string.Join(" ", param.Param);
                }
            }
        }

        // Save policy breaks
        if (request.Pb?.Infos != null)
        {
            foreach (var pb in request.Pb.Infos)
            {
                var existing = await db.SvPolicyBreaks.SingleOrDefaultAsync(p =>
                    p.RefId == refid && p.Version == gameVersion && p.Id1 == pb.Id);
                if (existing is null)
                {
                    db.SvPolicyBreaks.Add(new SvPolicyBreak
                    {
                        RefId = refid,
                        Version = gameVersion,
                        Id1 = pb.Id,
                        Exp = pb.Exp,
                    });
                }
                else
                {
                    existing.Exp = pb.Exp;
                }
            }
        }

        // Save story progression
        if (request.Story?.Infos != null)
        {
            foreach (var story in request.Story.Infos)
            {
                var existing = await db.Sv3Stories.SingleOrDefaultAsync(s =>
                    s.RefId == refid && s.Version == gameVersion && s.StoryId == story.StoryId);
                if (existing is null)
                {
                    db.Sv3Stories.Add(new Sv3Story
                    {
                        RefId = refid,
                        Version = gameVersion,
                        StoryId = story.StoryId,
                        ProgressId = story.ProgressId,
                        ProgressParam = story.ProgressParam,
                        ClearCnt = story.ClearCnt,
                        RouteFlg = story.RouteFlg,
                    });
                }
                else
                {
                    existing.ProgressId = story.ProgressId;
                    existing.ProgressParam = story.ProgressParam;
                    existing.ClearCnt = story.ClearCnt;
                    existing.RouteFlg = story.RouteFlg;
                }
            }
        }

        await db.SaveChangesAsync();
        return new SaveResponse { Status = "0" };
    }

    private async Task<SaveMResponse> SaveMSv3Internal(int gameVersion)
    {
        var request = Request as Sv3SaveMRequest;
        if (request is null) return new SaveMResponse { Status = "1" };

        var refid = request.RefId ?? request.DataId;
        if (string.IsNullOrEmpty(refid)) return new SaveMResponse { Status = "1" };

        using var db = new StellaKFCContext();

        var profile = await db.SvProfiles.SingleOrDefaultAsync(x => x.RefId == refid && x.Version == gameVersion);
        if (profile is null) return new SaveMResponse { Status = "1" };

        var existing = await db.SvScores.SingleOrDefaultAsync(s =>
            s.Profile == profile.Id && s.MusicId == request.MusicId && s.Type == request.MusicType && s.Version == gameVersion);

        if (existing is null)
        {
            db.SvScores.Add(new SvScore
            {
                Profile = profile.Id,
                MusicId = request.MusicId,
                Type = request.MusicType,
                Score = request.Score,
                Clear = request.ClearType,
                Grade = request.ScoreGrade,
                ButtonRate = request.BtnRate,
                LongRate = request.LongRate,
                VolRate = request.VolRate,
                PlayCount = 1,
                Version = gameVersion,
            });
        }
        else
        {
            if (request.Score > existing.Score)
            {
                existing.Score = request.Score;
                existing.ButtonRate = request.BtnRate;
                existing.LongRate = request.LongRate;
                existing.VolRate = request.VolRate;
            }
            existing.Clear = Math.Max(request.ClearType, existing.Clear);
            existing.Grade = Math.Max(request.ScoreGrade, existing.Grade);
            existing.PlayCount++;
        }

        await db.SaveChangesAsync();
        return new SaveMResponse { Status = "0" };
    }

    private async Task<SaveResponse> SaveCourseSv3Internal(int gameVersion)
    {
        var request = Request as SaveCourseRequest;
        if (request is null) return new SaveResponse { Status = "1" };

        var refid = request.Refid;
        if (string.IsNullOrEmpty(refid)) return new SaveResponse { Status = "1" };

        var course = request.Course;
        if (course is null) return new SaveResponse { Status = "1" };

        using var db = new StellaKFCContext();

        var profile = await db.SvProfiles.SingleOrDefaultAsync(x => x.RefId == refid && x.Version == gameVersion);
        if (profile is null) return new SaveResponse { Status = "1" };

        var existing = await db.SvCourseRecords.SingleOrDefaultAsync(c =>
            c.Profile == profile.Id && c.SeriesId == course.SeasonId && c.CourseId == course.CourseId && c.Version == gameVersion);

        if (existing is null)
        {
            db.SvCourseRecords.Add(new SvCourseRecord
            {
                Profile = profile.Id,
                SeriesId = course.SeasonId,
                CourseId = course.CourseId,
                Clear = course.Clear,
                Grade = course.Grade,
                Rate = course.Rate,
                Score = course.Score,
                Exscore = course.Exscore,
                Count = 1,
                Version = gameVersion,
            });
        }
        else
        {
            existing.Clear = (short)Math.Max(course.Clear, existing.Clear);
            existing.Grade = (short)Math.Max(course.Grade, existing.Grade);
            if (course.Score > existing.Score) existing.Score = course.Score;
            if (course.Exscore > existing.Exscore) existing.Exscore = course.Exscore;
            existing.Count++;
        }

        await db.SaveChangesAsync();
        return new SaveResponse { Status = "0" };
    }
}
