using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Stella.Abstractions.Plugins;
using StellaKFCPlugin.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using CorePlugin.EF;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using StellaKFCPlugin.Classes;
using StellaKFCPlugin.EF;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace StellaKFCPlugin.Handlers
{
    public class LoadHandlerNable : StellaHandler
    {
        [StellaHandler("game", "sv7_load", typeof(LoadRequest))]
        public async Task<LoadResponse> Load()
        {
            var request = Request as LoadRequest;
            var context = new StellaKFCContext();


            SvProfile? profile = await context.SvProfiles.SingleOrDefaultAsync(x =>
                x.RefId == request.Refid);
            if (profile is null)
            {
                Logger.LogInformation($"no profile data for RefId: {request.Refid}");
                return new LoadResponse() { Result = 1 };
            }

            var response = new LoadResponse()
            {
                AppealId = profile.AppealId,
                ArsOption = profile.ArsOption,
                BlasterCount = profile.BlasterCount,
                BlasterEnergy = profile.BlasterEnergy,
                BlockNo = profile.Pcb,
                Cloud = new Cloud() { Relation = 1 },
                Code = profile.Code,
                DayCount = profile.DayCount,
                DrawAdjust = profile.DrawAdjust,
                Eaappli = new Eaappli() { Relation = 1 },
                Name = profile.Name,
                EaShop = new EaShop()
                {
                    BlasterPassEnable = Convert.ToBoolean(profile.BlasterPassEnable),
                    BlasterPassLimitDate = profile.BlasterPassLimitDate,
                    PacketBooster = 1
                },
                EarlyLateDisp = profile.EarlyLateDisp,
                EffCLeft = profile.EffCLeft,
                EffCRight = profile.EffCRight,
                ExtrackEnergy = profile.ExtrackEnergy,
                GamecoinBlock = (uint)profile.Pcb,
                GamecoinPacket = 10000,
                GaugeOption = profile.GaugeOption,
                Headphone = profile.Headphone,
                Hispeed = profile.Hispeed,
                KacId = profile.KacId,
                Lanespeed = profile.Lanespeed,
                LastMusicId = profile.LastMusicId,
                LastMusicType = profile.LastMusicType,
                MaxPlayChain = profile.MaxPlayChain,
                MaxWeekChain = profile.MaxWeekChain,
                NarrowDown = 0,
                NotesOption = profile.NotesOption,
                PlayChain = profile.PlayChain,
                PlayCount = profile.PlayCount,
                SdvxId = profile.Code,
                TodayCount = profile.TodayCount,
                WeekPlayCount = profile.WeekPlayCount,
                WeekCount = profile.WeekCount,
                WeekChain = profile.WeekChain,
                SortType = profile.SortType,
                SkillNameId = profile.SkillNameId,
                SkillLevel = profile.SkillLevel,
                SkillBaseId = profile.SkillBaseId
            };
            var items = context.SvItems.Where(x => x.Profile == profile.Id).AsEnumerable();

            var param = context.SvParams.Where(x => x.Profile == profile.Id).AsEnumerable();

            var courses = context.SvCourseRecords.Where(x => x.Profile == profile.Id).AsEnumerable();

            var valgeneTicket = await context.SvValgeneTickets.SingleOrDefaultAsync(x =>
                x.Profile == profile.Id);

            response.Skill = new Skill();
            response.Skill.Course = courses.Select(x => new SkillCourse()
            {
                Ssnid = x.SeriesId,
                Crsid = x.CourseId,
                St = 0,
                Sc = x.Score,
                Ex = 0,
                Ct = x.Clear,
                Gr = x.Grade,
                Ar = x.Rate,
                Cnt = x.Count
            }).ToList();
            response.Item = new ItemElement();
            response.Item.Infos = items.Select(x => new ItemInfo()
            {
                Id = x.ItemId,
                Param = x.Param,
                Type = x.Type
            }).ToList();

            response.Param = new ParamElement();
            response.Param.Infos = param.Select(x => new ParamInfo()
            {
                Id = x.ParamId,
                Param = x.Param.Split(' ').Select(int.Parse).ToList(),
                Type = x.Type
            }).ToList();

            response.PlayCount = profile.PlayCount;
            response.DayCount = profile.DayCount;
            response.TodayCount = profile.TodayCount;
            response.PlayChain = profile.PlayChain;
            response.MaxPlayChain = profile.MaxPlayChain;
            response.WeekCount = profile.WeekCount;
            response.WeekPlayCount = profile.WeekPlayCount;
            response.WeekChain = profile.WeekChain;
            response.MaxWeekChain = profile.MaxWeekChain;

            response.ValgeneTicket = new ValgeneTicket();

            if (valgeneTicket is not null)
            {
                response.ValgeneTicket.TicketNum = valgeneTicket.TicketNum;
                response.ValgeneTicket.LimitDate = valgeneTicket.LimitDate;
            }

            return response;
        }

        [StellaHandler("game", "sv7_load_m", typeof(LoadMRequest))]
        public async Task<LoadMResponse> LoadM()
        {
            var request = Request as LoadMRequest;
            var context = new StellaKFCContext();
            var response = new LoadMResponse();

            SvProfile? profile = await context.SvProfiles.SingleOrDefaultAsync(x =>
                x.RefId == request.Refid);
            if (profile is null)
            {
                Logger.LogInformation($"no profile data for RefId: {request.Refid}");
                return new LoadMResponse() { Status = "1", Music = new MusicElement()};
            }

            var scores = context.SvScores.Where(x => x.Profile == profile.Id).AsEnumerable();

            response.Music = new MusicElement();
            foreach (var score in scores)
            {
                response.Music.Infos.Add(new MusicInfo
                {
                    Param = new List<uint>()
                    {
                        (uint)score.MusicId, (uint)score.Type, (uint)score.Score, (uint)score.Exscore,
                        (uint)score.Clear, (uint)score.Grade, 0, 0, (uint)score.ButtonRate, (uint)score.LongRate,
                        (uint)score.VolRate, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
                    }
                });

            }
            return response;
        }

        [StellaHandler("game", "sv7_load_r", typeof(LoadRivalRequest))]
        public async Task<LoadRivalResponse> LoadRival()
        {
            return new LoadRivalResponse();
        }
    }
}
