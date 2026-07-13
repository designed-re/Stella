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

namespace StellaKFCPlugin.Handlers
{
    /// <summary>
    /// Unified save handlers for EXCEED GEAR (sv6_*) and NABLA (sv7_*). Ported
    /// from asphyxia kfc/handlers/profiles.ts: save, saveScore, saveCourse,
    /// saveValgene, saveE, savePb, buy, print.
    /// </summary>
    public class SaveHandler : StellaHandler
    {
        // EG clear-lamp order: 0..6 = [0,1,2,3,6,4,5]. New id 6 (mxv) sits above 3.
        private static readonly int[] EgClearLampOrder = { 0, 1, 2, 3, 6, 4, 5 };

        [StellaHandler("game", "sv6_save_m", typeof(SaveMRequest))]
        public async Task<SaveMResponse> SaveMusic() => await SaveMusicInternal(6);

        [StellaHandler("game", "sv7_save_m", typeof(SaveMRequest))]
        public async Task<SaveMResponse> SaveMusicNabla() => await SaveMusicInternal(7);

        [StellaHandler("game", "sv6_save", typeof(SaveRequest))]
        public async Task<SaveResponse> Save() => await SaveInternal(6);

        [StellaHandler("game", "sv7_save", typeof(SaveRequest))]
        public async Task<SaveResponse> SaveNabla() => await SaveInternal(7);

        [StellaHandler("game", "sv6_save_e", typeof(SaveRequest))]
        public async Task<SaveResponse> SaveE() => new();

        [StellaHandler("game", "sv7_save_e", typeof(SaveRequest))]
        public async Task<SaveResponse> SaveENabla() => new();

        [StellaHandler("game", "sv6_save_c", typeof(SaveCourseRequest))]
        public async Task<SaveResponse> SaveCourse() => await SaveCourseInternal(6);

        [StellaHandler("game", "sv7_save_c", typeof(SaveCourseRequest))]
        public async Task<SaveResponse> SaveCourseNabla() => await SaveCourseInternal(7);

        [StellaHandler("game", "sv6_save_valgene", typeof(SaveValgeneRequest))]
        public async Task<SaveValgeneResponse> SaveValgene() => await SaveValgeneInternal(6);

        [StellaHandler("game", "sv7_save_valgene", typeof(SaveValgeneRequest))]
        public async Task<SaveValgeneResponse> SaveValgeneNabla() => await SaveValgeneInternal(7);

        [StellaHandler("game", "sv6_save_pb", typeof(SavePbRequest))]
        public async Task<SavePbResponse> SavePb() => await SavePbInternal(6);

        [StellaHandler("game", "sv7_save_pb", typeof(SavePbRequest))]
        public async Task<SavePbResponse> SavePbNabla() => await SavePbInternal(7);

        private async Task<SaveMResponse> SaveMusicInternal(int gameVersion)
        {
            var request = Request as SaveMRequest;
            if (request is null) return new SaveMResponse { Status = "1" };
            using var db = new StellaKFCContext();
            var response = new SaveMResponse();

            var profile = await db.SvProfiles.SingleOrDefaultAsync(x => x.RefId == request.RefId && x.Version == gameVersion);
            if (profile is null) { response.Status = "1"; return response; }

            var t = request.Track;
            var record = await db.SvScores.SingleOrDefaultAsync(x =>
                x.Profile == profile.Id && x.MusicId == t.MusicId && x.Type == t.MusicType && x.Version == gameVersion);
            bool isNew = record is null;
            record ??= new SvScore
            {
                MusicId = t.MusicId, Type = t.MusicType, Version = gameVersion, Profile = profile.Id,
                Score = 0, Exscore = 0, Clear = 0, Grade = 0,
                ButtonRate = 0, LongRate = 0, VolRate = 0, Volforce = 0, PlayCount = 0,
            };

            if (t.Score > record.Score)
            {
                record.Score = t.Score;
                record.ButtonRate = t.BtnRate;
                record.LongRate = t.LongRate;
                record.VolRate = t.VolRate;
            }
            if (t.ExScore > record.Exscore) record.Exscore = t.ExScore;

            // Clear lamp handling: EG uses a non-monotonic order; NABLA is chronological.
            if (gameVersion == 6)
            {
                int newClear = t.ClearType;
                int oldClear = record.Clear;
                bool newIsGreater = Array.IndexOf(EgClearLampOrder, newClear) > Array.IndexOf(EgClearLampOrder, oldClear);
                record.Clear = newIsGreater ? newClear : oldClear;
            }
            else
            {
                record.Clear = Math.Max(t.ClearType, record.Clear);
            }
            record.Grade = Math.Max(t.ScoreGrade, record.Grade);

            if (gameVersion == 7)
            {
                int volforce = 0; // game sends volforce via track? asphyxia reads i.number('volforce', 0)
                if (volforce > record.Volforce) record.Volforce = volforce;
            }
            record.PlayCount++;

            if (isNew) db.SvScores.Add(record);
            else db.SvScores.Update(record);
            await db.SaveChangesAsync();
            return response;
        }

        private async Task<SaveResponse> SaveInternal(int gameVersion)
        {
            var request = Request as SaveRequest;
            if (request is null) return new SaveResponse { Status = "1" };
            using var db = new StellaKFCContext();
            var response = new SaveResponse();

            var profile = await db.SvProfiles.SingleOrDefaultAsync(x => x.RefId == request.RefId && x.Version == gameVersion);
            if (profile is null) { response.Status = "1"; return response; }

            // Profile $set (asphyxia save L534-572).
            profile.AppealId = request.AppealId;
            profile.LastMusicId = request.MusicId;
            profile.LastMusicType = request.MusicType;
            profile.SortType = request.SortType;
            profile.Headphone = request.Headphone;
            profile.Hispeed = request.HiSpeed;
            profile.Lanespeed = request.LaneSpeed;
            profile.GaugeOption = request.GaugeOption;
            profile.ArsOption = request.ArsOption;
            profile.NotesOption = request.NotesOption;
            profile.EarlyLateDisp = request.EarlyLateDisp;
            profile.DrawAdjust = request.DrawAdjust;
            profile.EffCLeft = request.EffCLeft;
            profile.EffCRight = request.EffCRight;

            // Profile $inc.
            profile.Pcb += request.EarnedGamecoinPacket + request.EarnedGamecoinBlock;
            profile.BlasterEnergy += (uint)request.EarnedBlasterEnergy;
            profile.PlayCount++;
            profile.DayCount++;
            profile.TodayCount++;
            profile.PlayChain++;
            profile.MaxPlayChain++;
            profile.WeekCount++;
            profile.WeekPlayCount++;
            profile.WeekChain++;
            profile.MaxWeekChain++;
            db.SvProfiles.Update(profile);

            // Course record (asphyxia save L578-607): course element with ssnid/crsid/st/kac_id.
            if (request.Course is not null)
            {
                var c = request.Course;
                var rec = await db.SvCourseRecords.SingleOrDefaultAsync(x =>
                    x.Profile == profile.Id && x.SeriesId == c.SeasonId && x.CourseId == c.CourseId &&
                    x.SkillType == 0 && x.Version == gameVersion);
                if (rec is null)
                {
                    db.SvCourseRecords.Add(new SvCourseRecord
                    {
                        Profile = profile.Id, SeriesId = c.SeasonId, CourseId = c.CourseId,
                        SkillType = 0, Version = gameVersion, Score = c.Score, Exscore = 0,
                        Clear = c.Clear, Grade = c.Grade, Rate = c.Rate, Count = 1,
                    });
                }
                else
                {
                    rec.Score = Math.Max(c.Score, rec.Score);
                    rec.Clear = Math.Max(c.Clear, rec.Clear);
                    rec.Grade = Math.Max(c.Grade, rec.Grade);
                    rec.Rate = Math.Max(c.Rate, rec.Rate);
                    rec.Count++;
                    db.SvCourseRecords.Update(rec);
                }
            }

            // Items (asphyxia save L610-624).
            foreach (var item in request.Item.Infos)
            {
                var rec = await db.SvItems.AsNoTracking().SingleOrDefaultAsync(x =>
                    x.Profile == profile.Id && x.ItemId == item.Id && x.Type == item.Type && x.Version == gameVersion);
                if (rec is null)
                    await db.SvItems.AddAsync(new SvItem { ItemId = item.Id, Param = item.Param, Type = item.Type, Profile = profile.Id, Version = gameVersion });
                else
                    db.SvItems.Update(new SvItem { Id = rec.Id, ItemId = item.Id, Param = item.Param, Type = item.Type, Profile = profile.Id, Version = gameVersion });
            }

            // Params (asphyxia save L627-640).
            foreach (var p in request.ParamElement.Infos)
            {
                var rec = await db.SvParams.AsNoTracking().SingleOrDefaultAsync(x =>
                    x.Profile == profile.Id && x.ParamId == p.Id && x.Type == p.Type && x.Version == gameVersion);
                var paramStr = string.Join(' ', p.Params);
                if (rec is null)
                    await db.SvParams.AddAsync(new SvParam { ParamId = p.Id, Param = paramStr, Type = p.Type, Profile = profile.Id, ParamCount = (uint)p.Params.Count, Version = gameVersion });
                else
                    db.SvParams.Update(new SvParam { Id = rec.Id, ParamId = p.Id, Param = paramStr, Type = p.Type, Profile = profile.Id, ParamCount = (uint)p.Params.Count, Version = gameVersion });
            }

            // Skill (asphyxia save L643-658): separate SvSkill row.
            var skill = await db.SvSkills.SingleOrDefaultAsync(s => s.Profile == profile.Id && s.Version == gameVersion);
            if (skill is null)
            {
                db.SvSkills.Add(new SvSkill
                {
                    Profile = profile.Id, Version = gameVersion,
                    Base = request.SkillBaseId, Level = request.SkillLevel,
                    Name = request.SkillNameId, Type = request.SkillType,
                });
            }
            else
            {
                skill.Base = request.SkillBaseId;
                skill.Level = request.SkillLevel;
                skill.Name = request.SkillNameId;
                skill.Type = request.SkillType;
                db.SvSkills.Update(skill);
            }

            // Arena (asphyxia save L661-693): not present in SaveRequest model yet.
            // (Request model would need an `arena` element; left for follow-up.)

            // Variant gate (asphyxia save L696-724): present in SaveRequest as VariantGate.
            if (request.VariantGate is not null)
            {
                var vg = request.VariantGate;
                var vp = await db.SvVariantPowers.SingleOrDefaultAsync(v => v.Profile == profile.Id && v.Version == gameVersion);
                if (vp is null)
                {
                    vp = new SvVariantPower { Profile = profile.Id, Version = gameVersion };
                    db.SvVariantPowers.Add(vp);
                }
                else
                {
                    db.SvVariantPowers.Update(vp);
                }
                vp.Power += vg.EarnedPower;
                if (vg.EarnedElement is not null)
                {
                    vp.Notes += vg.EarnedElement.Notes;
                    vp.Peak += vg.EarnedElement.Peak;
                    vp.Tsumami += vg.EarnedElement.Tsumami;
                    vp.Tricky += vg.EarnedElement.Tricky;
                    vp.Onehand += vg.EarnedElement.OneHand;
                    vp.Handtrip += vg.EarnedElement.HandTrip;
                }
            }

            await db.SaveChangesAsync();
            return response;
        }

        private async Task<SaveResponse> SaveCourseInternal(int gameVersion)
        {
            var request = Request as SaveCourseRequest;
            if (request is null) return new SaveResponse { Status = "1" };
            using var db = new StellaKFCContext();
            var profile = await db.SvProfiles.SingleOrDefaultAsync(x => x.RefId == request.Refid && x.Version == gameVersion);
            if (profile == null || request.Course is null) return new SaveResponse { Status = "1" };

            var c = request.Course;
            var rec = await db.SvCourseRecords.SingleOrDefaultAsync(x =>
                x.Profile == profile.Id && x.SeriesId == c.SeasonId && x.CourseId == c.CourseId && x.Version == gameVersion);
            if (rec is null)
            {
                db.SvCourseRecords.Add(new SvCourseRecord
                {
                    Profile = profile.Id, SeriesId = c.SeasonId, CourseId = c.CourseId,
                    Version = gameVersion, Score = c.Score, Exscore = 0,
                    Clear = c.Clear, Grade = c.Grade, Rate = c.Rate, Count = 1,
                });
            }
            else
            {
                rec.Score = Math.Max(c.Score, rec.Score);
                rec.Clear = Math.Max(c.Clear, rec.Clear);
                rec.Grade = Math.Max(c.Grade, rec.Grade);
                rec.Rate = Math.Max(c.Rate, rec.Rate);
                rec.Count++;
                db.SvCourseRecords.Update(rec);
            }
            await db.SaveChangesAsync();
            return new SaveResponse();
        }

        private async Task<SaveValgeneResponse> SaveValgeneInternal(int gameVersion)
        {
            var request = Request as SaveValgeneRequest;
            if (request is null) return new SaveValgeneResponse { Status = "1" };
            using var db = new StellaKFCContext();
            var profile = await db.SvProfiles.SingleOrDefaultAsync(x => x.RefId == request.RefId && x.Version == gameVersion);
            if (profile == null) return new SaveValgeneResponse { Status = "1" };

            // Stamp items (type 17) get expanded into 4 entries (asphyxia saveValgene L1216-1223).
            var itemsToAdd = new List<(int type, uint id, uint param)>();
            foreach (var item in request.Item.Infos)
            {
                int type = (int)item.Type;
                int id = (int)item.Id;
                uint param = item.Param;
                if (type == 17)
                {
                    for (int stampId = (id * 4) - 3; stampId <= (id * 4); stampId++)
                        itemsToAdd.Add((type, (uint)stampId, param));
                }
                else
                {
                    itemsToAdd.Add((type, (uint)id, param));
                }
            }

            foreach (var (type, id, param) in itemsToAdd)
            {
                var rec = await db.SvItems.SingleOrDefaultAsync(x =>
                    x.Profile == profile.Id && x.ItemId == id && x.Type == (byte)type && x.Version == gameVersion);
                if (rec is null)
                {
                    db.SvItems.Add(new SvItem { ItemId = id, Param = param, Type = (byte)type, Profile = profile.Id, Version = gameVersion });
                }
                else
                {
                    rec.Param = param;
                    db.SvItems.Update(rec);
                }

                // asphyxia saveValgene L1237-1243: v7 also writes id<=18 back to v6.
                if (gameVersion == 7 && id <= 18)
                {
                    var rec6 = await db.SvItems.SingleOrDefaultAsync(x =>
                        x.Profile == profile.Id && x.ItemId == id && x.Type == (byte)type && x.Version == 6);
                    if (rec6 is null)
                        db.SvItems.Add(new SvItem { ItemId = id, Param = param, Type = (byte)type, Profile = profile.Id, Version = 6 });
                    else
                    {
                        rec6.Param = param;
                        db.SvItems.Update(rec6);
                    }
                }
            }

            // Ticket decrement.
            if (request.UseTicket)
            {
                var ticket = await db.SvValgeneTickets.SingleOrDefaultAsync(x => x.Profile == profile.Id);
                if (ticket is not null)
                {
                    ticket.TicketNum--;
                    db.SvValgeneTickets.Update(ticket);
                }
            }
            await db.SaveChangesAsync();

            var response = new SaveValgeneResponse { Result = 1 };
            var resultTicket = await db.SvValgeneTickets.SingleOrDefaultAsync(x => x.Profile == profile.Id);
            if (resultTicket is not null)
            {
                response.TicketNum = resultTicket.TicketNum;
                response.LimitDate = resultTicket.LimitDate;
            }
            return response;
        }

        private async Task<SavePbResponse> SavePbInternal(int gameVersion)
        {
            var request = Request as SavePbRequest;
            if (request is null) return new SavePbResponse { Status = "1" };
            using var db = new StellaKFCContext();
            // asphyxia savePb: upsert policy_break exp, reward at 24000.
            var pb = await db.SvPolicyBreaks.SingleOrDefaultAsync(p =>
                p.RefId == request.RefId && p.Version == gameVersion && p.Id1 == request.Id);
            if (pb is null)
            {
                pb = new SvPolicyBreak { RefId = request.RefId, Version = gameVersion, Id1 = request.Id, Exp = request.Exp };
                db.SvPolicyBreaks.Add(pb);
            }
            else
            {
                pb.Exp = request.Exp;
                db.SvPolicyBreaks.Update(pb);
            }
            await db.SaveChangesAsync();

            // Reward when exp >= 24000 (asphyxia savePb L1338-1345). Lookup SvPolicyBreakData.
            var pbData = await db.SvPolicyBreakDatas.FirstOrDefaultAsync(p => p.Version == gameVersion && p.Pbid == request.Id);
            if (pbData is not null && pb.Exp >= 24000)
            {
                var profile = await db.SvProfiles.SingleOrDefaultAsync(x => x.RefId == request.RefId && x.Version == gameVersion);
                if (profile is not null)
                {
                    var item = await db.SvItems.SingleOrDefaultAsync(x =>
                        x.Profile == profile.Id && x.Type == (byte)pbData.RwrdType && x.ItemId == (uint)pbData.RwrdId && x.Version == gameVersion);
                    if (item is null)
                        db.SvItems.Add(new SvItem { Profile = profile.Id, Type = (byte)pbData.RwrdType, ItemId = (uint)pbData.RwrdId, Param = (uint)pbData.RwrdParam, Version = gameVersion });
                    else
                    {
                        item.Param = (uint)pbData.RwrdParam;
                        db.SvItems.Update(item);
                    }
                    await db.SaveChangesAsync();
                }
            }

            return new SavePbResponse { Exp = request.Exp, Result = true };
        }
    }
}