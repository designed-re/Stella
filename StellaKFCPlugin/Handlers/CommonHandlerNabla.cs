using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Stella.Abstractions;
using Stella.Abstractions.Plugins;
using StellaKFCPlugin.Classes;
using StellaKFCPlugin.EF;
using StellaKFCPlugin.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace StellaKFCPlugin.Handlers
{
    public class CommonHandlerNable : StellaHandler
    {
        [StellaHandler("game", "sv7_common", typeof(GetCommonRequest))]
        public async Task<GetCommonResponse> GetCommon()
        {
            var request = Request as GetCommonRequest;
            var context = new StellaKFCContext();
            try
            {
                var events = await context.SvEvents.Where(e => e.Enabled).Select(x => x.Event).ToListAsync();

                Valgene valgeneData = JsonConvert.DeserializeObject<Valgene>(
                    System.IO.File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "exd_valgene.json")));
                Course[] courseData = JsonConvert.DeserializeObject<Course[]>(
                    System.IO.File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "exd_course.json")));
                JObject megamixData = JObject.Parse(
                    System.IO.File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data", "exd_megamix.json")));
                CurrentArena currentArenaData = JsonConvert.DeserializeObject<CurrentArena>(
                    System.IO.File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data",
                        "current_arena.json")));
                JObject arenaData = JObject.Parse(
                    System.IO.File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Data",
                        "exd_arena.json")));

                var response = new GetCommonResponse() {Status="0"};
                ValgeneElement valgeneElement = new ValgeneElement();
                
                // Add valgene info
                foreach (var info in valgeneData.Info)
                {
                    valgeneElement.Infos.Add(new ValgeneInfo
                    {
                        ValgeneName = info.ValgeneName,
                        ValgeneNameEnglish = info.ValgeneNameEnglish,
                        ValgeneId = Convert.ToInt32(info.ValgeneId)
                    });
                }

                // Add valgene catalogs
                foreach (var catalog in valgeneData.Catalog)
                {
                    foreach (var catalogItem in catalog.Items)
                    {
                        foreach (var ids in catalogItem.ItemIds)
                        {
                            valgeneElement.Catalogs.Add(new ValgenesCatalog
                            {
                                ValgeneId = Convert.ToInt32(catalog.Volume),
                                ItemType = Convert.ToInt32(catalogItem.Type),
                                ItemId = Convert.ToInt32(ids),
                                Rarity = Convert.ToInt32(valgeneData.Rarity[catalogItem.Type.ToString()])
                            });
                        }
                    }
                }

                response.Valgene = valgeneElement;

                SkillCourseElement skillCourseElement = new SkillCourseElement();

                // Extract model version and cab type from request
                var trimedVersion = Model.Substring(10, 8);
                var currentVersion = int.Parse(trimedVersion.Length > 0 ? trimedVersion : "0");
                string cabType = Model.Length > 2 ? Model[2].ToString() : "A"; // Default A

                // Process courses
                foreach (var s in courseData)
                {
                    if (currentVersion >= s.Version)
                    {
                        // Helper function to add courses
                        void AddCoursesToModel(CourseElement[] courses, short skillType)
                        {
                            foreach (var c in courses)
                            {
                                var courseInfo = new CourseInfo
                                {
                                    SeasonId = s.Id,
                                    SeasonName = s.Name,
                                    SeasonNewFlg = s.IsNew == 1,
                                    CourseType = c.Type,
                                    CourseId = c.Id,
                                    CourseName = c.Name,
                                    SkillLevel = c.Level,
                                    SkillType = skillType,
                                    SkillNameId = c.NameId,
                                    MatchingAssist = c.Assist == 1,
                                    ClearRate = 5000,
                                    AvgScore = 15000000
                                };

                                // Add tracks
                                foreach (var t in c.Tracks)
                                {
                                    courseInfo.Tracks.Add(new TrackInfo
                                    {
                                        TrackNo = (short)t.No,
                                        MusicId = (int)t.Mid,
                                        MusicType = (sbyte)t.Mty
                                    });
                                }

                                skillCourseElement.Infos.Add(courseInfo);
                            }
                        }

                        AddCoursesToModel(s.Courses, 0);

                        // Add God courses for specific cab types
                        if ((cabType == "G" || cabType == "H") && s.HasGod == 1 && currentVersion >= 20230530)
                        {
                            AddCoursesToModel(s.Courses, s.HasGod);
                        }
                    }
                }

                response.SkillCourse = skillCourseElement;

                EventElement eventElement = new EventElement();
                foreach (var @event in events)
                {
                    eventElement.Infos.Add(new EventInfo(){EventId = @event});
                }
                response.Event = eventElement;

                ArenaElement arenaElement = new ArenaElement();

                // Get config from PluginConfig
                var kfcConfig = PluginConfig as StellaKFCPluginConfig;

                // Check if arena should be open
                if (kfcConfig.ArenaOpen && currentVersion >= 20220425 && currentArenaData.Season != 0)
                {
                    // Add arena basic info
                    arenaElement.Season = currentArenaData.Season;
                    arenaElement.Rule = currentArenaData.Rule;
                    arenaElement.RankMatchTarget = currentArenaData.RankMatchTarget;
                    arenaElement.TimeStart = (ulong)currentArenaData.TimeStart.ToUnixTimeMilliseconds();
                    arenaElement.TimeEnd = (ulong)currentArenaData.TimeEnd.ToUnixTimeMilliseconds();
                    arenaElement.ShopStart = (ulong)currentArenaData.ShopStart.ToUnixTimeMilliseconds();
                    arenaElement.ShopEnd = (ulong)currentArenaData.ShopEnd.ToUnixTimeMilliseconds();

                    // Calculate arena open status
                    long currentTimeMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    bool isOpen = kfcConfig.ArenaOpen && currentTimeMs < (long)currentArenaData.TimeEnd.ToUnixTimeMilliseconds();
                    bool shopOpen = kfcConfig.ArenaOpen &&
                                   currentTimeMs >= (long)currentArenaData.ShopStart.ToUnixTimeMilliseconds() &&
                                   currentTimeMs < (long)currentArenaData.ShopEnd.ToUnixTimeMilliseconds();

                    arenaElement.IsOpen = isOpen;
                    arenaElement.IsShop = shopOpen;

                    // Process arena shop catalog if shop is open
                    if (shopOpen && kfcConfig.ArenaSession != 0 && arenaData["Set " + kfcConfig.ArenaSession] != null)
                    {
                        var stationData = arenaData["Set " + kfcConfig.ArenaSession];
                        int stationVersion = stationData["version"]?.Value<int>() ?? 0;

                        if (currentVersion >= stationVersion)
                        {
                            if (stationData["items"] is JArray items)
                            {
                                foreach (var itemArray in items)
                                {
                                    if (itemArray is JArray { Count: >= 6 } itemElements)
                                    {
                                        arenaElement.Catalogs.Add(new ArenaCatalog
                                        {
                                            CatalogId = itemElements[0].Value<int>(),
                                            CatalogType = itemElements[1].Value<int>(),
                                            Price = itemElements[2].Value<int>(),
                                            ItemType = itemElements[3].Value<int>(),
                                            ItemId = itemElements[4].Value<int>(),
                                            Param = itemElements[5].Value<int>()
                                        });
                                    }
                                }
                            }
                        }
                    }
                }

                response.Arena = arenaElement;

                ExtendElement extendElement = new ExtendElement();

                // Add megamix data as extend features
                uint extendId = 91;
                for (int i = 1; i <= 4; i++)
                {
                    var megamix = megamixData[$"megamix{i}"];

                    var extendInfo = new ExtendInfo
                    {
                        ExtendId = extendId++,
                        ExtendType = 17,
                        ParamNum1 = 0,
                        ParamNum2 = 0,
                        ParamNum3 = 0,
                        ParamNum4 = 0,
                        ParamNum5 = 0,
                        ParamStr1 = string.Join(",", megamix.Values<int>()),
                        ParamStr2 = "",
                        ParamStr3 = "",
                        ParamStr4 = "",
                        ParamStr5 = ""
                    };
                    extendElement.Infos.Add(extendInfo);
                }

                // Add notification extend
                var notificationTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var notiInfo = new ExtendInfo
                {
                    ExtendId = 1,
                    ExtendType = 1,
                    ParamNum1 = 1,
                    ParamNum2 = Convert.ToInt32(notificationTime / 100000),
                    ParamNum3 = 0,
                    ParamNum4 = 0,
                    ParamNum5 = 0,
                    ParamStr1 = $"[f:0] NOTIFICATION\nFREE SOFTWARE\n{DateTime.Now:s}",
                    ParamStr2 = "",
                    ParamStr3 = "",
                    ParamStr4 = "",
                    ParamStr5 = ""
                };
                extendElement.Infos.Add(notiInfo);

                response.Extend = extendElement;

                MusicLimitedElement musicLimitedElement = new MusicLimitedElement();

                int lastSongId = context.SvMusics.OrderBy(x => x.Id).Last().Id;

                // Process music limitations based on config
                if (kfcConfig.UnlockAllSongs)
                {
                    // Unlock all songs - set all to limited=3 (fully available)
                    for (int i = 1; i <= lastSongId; i++)
                    {
                        // Add all 5 music types for each song
                        for (byte musicType = 0; musicType < 5; musicType++)
                        {
                            musicLimitedElement.Infos.Add(new MusicLimitedInfo
                            {
                                MusicId = i,
                                MusicType = musicType,
                                Limited = 3  // Fully available
                            });
                        }
                    }
                }
                else
                {
                    // Selective song availability
                    var licensedSongs = new int[] { };
                    var valkyrieSongs = new int[] { };

                    for (int i = 0; i <= lastSongId; i++)
                    {
                        var songData = context.SvMusics.FirstOrDefault(s => s.Id == i);

                        if (songData != null)
                        {
                            int limitedNo = 2;

                            // Apply logic based on game version
                            if (Math.Abs(currentVersion) == 6)
                            {
                                limitedNo = 2;

                                // Adjust limitation based on song type
                                if (licensedSongs.Contains(i))
                                    limitedNo += 1;
                                else if (valkyrieSongs.Contains(i) && !System.Text.RegularExpressions.Regex.IsMatch(cabType, @"^(G|H)$"))
                                    limitedNo -= 1;

                                // Special case for specific song
                                if (i == 2034)
                                    limitedNo = 2;

                                // Add all 6 difficulty types for this song
                                for (byte difficultyType = 0; difficultyType < 6; difficultyType++)
                                {
                                    musicLimitedElement.Infos.Add(new MusicLimitedInfo
                                    {
                                        MusicId = i,
                                        MusicType = difficultyType,
                                        Limited = (byte)limitedNo
                                    });
                                }
                            }
                        }
                    }
                }

                response.MusicLimited = musicLimitedElement;


                // TODO: Implement complete game common data response
                // Current implementation provides basic structure
                // Future enhancements needed:
                // 1. Load valgene (gacha) data from JSON or database
                // 2. Load skill course data
                // 3. Load event data from database
                // 4. Load arena data from configuration
                // 5. Load music limitations from database
                // 6. Add extend/feature data
                // 7. Build complete XML response structure

                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                // Rethrow with handler exception
                throw new StellaHandlerException(500);
            }
        }

        // TODO: Implement helper methods for:
        // - Loading valgene data (gacha system)
       
    }
}


