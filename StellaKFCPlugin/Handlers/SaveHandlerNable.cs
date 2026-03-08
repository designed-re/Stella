using CorePlugin.EF;
using CorePlugin.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Stella.Abstractions;
using Stella.Abstractions.Plugins;
using Stella.Util.KBinXML;
using StellaKFCPlugin.Classes;
using StellaKFCPlugin.EF;
using StellaKFCPlugin.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Xml.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StellaKFCPlugin.Handlers
{
    public class SaveHandlerNable : StellaHandler
    {
        [StellaHandler("game", "sv7_save_m", typeof(SaveMRequest))]
        public async Task<SaveMResponse> SaveMusic()
        {
            var request = Request as SaveMRequest;

            var context = new StellaKFCContext();

            var response = new SaveMResponse();

            var profile = context.SvProfiles.SingleOrDefault(x => x.RefId == request.RefId);
            if (profile is null)
            {
                response.Status = "1";
                return response;
            }

            SvScore record =
                await context.SvScores.SingleOrDefaultAsync(x => x.Profile == profile.Id && x.MusicId == request.Track.MusicId && x.Type == request.Track.MusicType) ?? new()
                {
                    MusicId = request.Track.MusicId,
                    Type = request.Track.MusicType,
                    Score = 0,
                    Exscore = 0,
                    Clear = 0,
                    Grade = 0,
                    ButtonRate = 0,
                    LongRate = 0,
                    VolRate = 0,
                    Profile = profile.Id
                };
            if (request.Track.Score > record.Score)
            {
                record.Score = request.Track.Score;
                record.ButtonRate = request.Track.BtnRate;
                record.LongRate = request.Track.LongRate;
                record.VolRate = request.Track.VolRate;
            }

            if (request.Track.ExScore > record.Exscore) record.Exscore = request.Track.ExScore;

            record.Clear = Math.Max(request.Track.ClearType, record.Clear);
            record.Grade = Math.Max(request.Track.ScoreGrade, record.Grade);

            var record1 = await context.SvScores.SingleOrDefaultAsync(x =>
                x.Profile == profile.Id && x.MusicId == request.Track.MusicId && x.Type == request.Track.MusicType);

            if (record1 is null)
                context.SvScores.Add(record);
            else
                context.SvScores.Update(record);

            await context.SaveChangesAsync();

            return response;
        }

        [StellaHandler("game", "sv7_save", typeof(SaveRequest))]
        public async Task<SaveResponse> Save()
        {
            var request = Request as SaveRequest;

            var context = new StellaKFCContext();

            var response = new SaveResponse();

            var profile = context.SvProfiles.SingleOrDefault(x => x.RefId == request.RefId);
            if (profile is null)
            {
                response.Status = "1";
                return response;
            }

            profile.AppealId = request.AppealId;
            profile.SkillLevel = request.SkillLevel;
            profile.SkillBaseId = request.SkillBaseId;
            profile.SkillNameId = request.SkillNameId;
            profile.Hispeed = request.HiSpeed;
            profile.Lanespeed = request.LaneSpeed;
            profile.GaugeOption = request.GaugeOption;
            profile.ArsOption = request.ArsOption;
            profile.NotesOption = request.NotesOption;
            profile.EarlyLateDisp = request.EarlyLateDisp;
            profile.DrawAdjust = request.DrawAdjust;
            profile.EffCLeft = request.EffCLeft;
            profile.EffCRight = request.EffCRight;
            profile.LastMusicId = request.MusicId;
            profile.LastMusicType = request.MusicType;
            profile.SortType = request.SortType;
            profile.Headphone = request.Headphone;


            profile.Pcb += (request.EarnedGamecoinPacket + request.EarnedGamecoinBlock);
            profile.BlasterEnergy += (uint)request.EarnedBlasterEnergy;

            profile.PlayCount++;
            profile.DayCount++;
            profile.TodayCount++;
            profile.PlayChain++;
            profile.MaxPlayChain++;
            profile.WeekChain++;
            profile.WeekCount++;
            profile.WeekPlayCount++;
            profile.MaxWeekChain++;

            if (request.Course is not null)
            {
                var courseRecord = await context.SvCourseRecords.SingleOrDefaultAsync(x =>
                    x.Profile == profile.Id && x.SeriesId == request.Course.SeasonId &&
                    x.CourseId == request.Course.CourseId);

                if (courseRecord is null)
                {
                    context.SvCourseRecords.Add(new()
                    {
                        Profile = profile.Id,
                        SeriesId = request.Course.SeasonId,
                        CourseId = request.Course.CourseId,
                        Version = 0,
                        Score = request.Course.Score,
                        Clear = request.Course.Clear,
                        Grade = request.Course.Grade,
                        Rate = request.Course.Rate,
                        Count = 1
                    });
                }
                else
                {
                    courseRecord.Score = Math.Max(request.Course.Score, courseRecord.Score);
                    courseRecord.Clear = Math.Max(request.Course.Clear, courseRecord.Clear);
                    courseRecord.Grade = Math.Max(request.Course.Grade, courseRecord.Grade);
                    courseRecord.Rate = Math.Max(request.Course.Rate, courseRecord.Rate);
                    courseRecord.Count++;
                    context.SvCourseRecords.Update(courseRecord);
                }
            }

            var items = request.Item.Infos;

            foreach (var item in items)
            {
                var record = await context.SvItems.AsNoTracking().SingleOrDefaultAsync(x => x.Profile == profile.Id && x.ItemId == item.Id && x.Type == item.Type);

                if (record is null)
                {
                    await context.SvItems.AddAsync(new() { ItemId = item.Id, Param = item.Param, Type = item.Type, Profile = profile.Id });
                }
                else
                {
                    context.SvItems.Update(new() { Id = record.Id, ItemId = item.Id, Param = item.Param, Type = item.Type, Profile = profile.Id });
                }
            }

            var param = request.ParamElement.Infos;
            foreach (var p in param)
            {
                var record = await context.SvParams.AsNoTracking().SingleOrDefaultAsync(x => x.Profile == profile.Id && x.ParamId == p.Id && x.Type == p.Type);

                if (record is null)
                {
                    await context.SvParams.AddAsync(new() { ParamId = p.Id, Param = string.Join(' ', p.Params), Type = p.Type, Profile = profile.Id });
                }
                else
                {
                    context.SvParams.Update(new() { Id = record.Id, ParamId = p.Id, Param = string.Join(' ', p.Params), Type = p.Type, Profile = profile.Id });
                }
            }

            context.SvProfiles.Update(profile);
            await context.SaveChangesAsync();


            return response;
        }

        [StellaHandler("game", "sv7_save_e", typeof(SaveRequest))]
        public async Task<SaveResponse> SaveExtra()
        {
            return new SaveResponse();
        }

        [StellaHandler("game", "sv7_save_c", typeof(SaveCourseRequest))]
        public async Task<SaveResponse> SaveCourse()
        {
            var request = Request as SaveCourseRequest;
            var context = new StellaKFCContext();

            var profile = context.SvProfiles.SingleOrDefault(x => x.RefId == request.Refid);

            if (profile == null || request.Course is null)
            {
                return new SaveResponse() { Status = "1" };
            }

            var course = request.Course;

            var record = await context.SvCourseRecords.SingleOrDefaultAsync(x =>
                x.Profile == profile.Id && x.SeriesId == course.SeasonId && x.CourseId == course.CourseId);

            if (record is null)
            {
                context.SvCourseRecords.Add(new()
                {
                    Profile = profile.Id,
                    SeriesId = course.SeasonId,
                    CourseId = course.CourseId,
                    Version = 0,
                    Score = course.Score,
                    Clear = course.Clear,
                    Grade = course.Grade,
                    Rate = course.Grade,
                    Count = 1
                });
            }
            else
            {
                record.Score = Math.Max(course.Score, record.Score);
                record.Clear = Math.Max(course.Clear, record.Clear);
                record.Grade = Math.Max(course.Grade, record.Grade);
                record.Rate = Math.Max(course.Rate, record.Rate);
                record.Count++;
                context.SvCourseRecords.Update(record);
            }

            await context.SaveChangesAsync();
            return new SaveResponse();
        }

        [StellaHandler("game", "sv7_save_valgene", typeof(SaveValgeneRequest))]
        public async Task<SaveValgeneResponse> SaveValgene()
        {
            var request = Request as SaveValgeneRequest;
            var context = new StellaKFCContext();
            var profile = context.SvProfiles.SingleOrDefault(x => x.RefId == request.RefId);
            
            if (profile == null)
            {
                return new SaveValgeneResponse() { Status = "1" };
            }

            var items = request.Item.Infos;


            bool useTicket = request.UseTicket;

            var itemsToAdd = new List<(int type, uint id, uint param)>();

            foreach (var itemElement in items)
            {
                int type = (int)itemElement.Type;
                int id = (int)itemElement.Id;
                uint param = itemElement.Param;

                if (type == 17)
                {
                    // Special handling for stamp items: create 4 entries
                    for (int stampId = (id * 4) - 3; stampId <= (id * 4); stampId++)
                    {
                        itemsToAdd.Add((type, Convert.ToUInt32(stampId), param));
                    }
                }
                else
                {
                    itemsToAdd.Add((type, Convert.ToUInt32(id), param));
                }
            }

            foreach (var (type, id, param) in itemsToAdd)
            {
                var item = await context.SvItems.SingleOrDefaultAsync(x =>
                    x.Profile == profile.Id && x.ItemId == id && x.Type == type);

                if (item is null)
                {
                    context.SvItems.Add(new()
                    {
                        ItemId = id,
                        Param = param,
                        Type = (byte)type,
                        Profile = profile.Id
                    });
                }
                else
                {
                    item.Param = param;
                    context.SvItems.Update(item);
                }
            }

            if (useTicket)
            {
                var ticket = await context.SvValgeneTickets.SingleOrDefaultAsync(x =>
                    x.Profile == profile.Id);

                if (ticket is not null)
                {
                    ticket.TicketNum--;
                    context.SvValgeneTickets.Update(ticket);
                }
            }
            await context.SaveChangesAsync();
            var response = new SaveValgeneResponse();
            var resultTicket = await context.SvValgeneTickets.SingleOrDefaultAsync(x =>
                x.Profile == profile.Id);

            if (resultTicket is not null)
            {
                response.Result = 1;
                response.TicketNum = resultTicket.TicketNum;
                response.LimitDate = resultTicket.LimitDate;
            }

            return response;
        }
    }

}
