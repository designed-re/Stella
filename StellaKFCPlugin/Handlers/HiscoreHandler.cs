using CorePlugin.EF;
using CorePlugin.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Stella.Abstractions.Plugins;
using StellaKFCPlugin.EF;
using StellaKFCPlugin.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Stella.Abstractions;

namespace StellaKFCPlugin.Handlers
{
    public class HiscoreHandler : StellaHandler
    {
        [StellaHandler("game", "sv6_hiscore", typeof(HiscoreRequest))]
        public async Task<HiscoreResponse> Hiscore()
        {

            var context = new StellaKFCContext();
            var response = new HiscoreResponse();

            var allScores = await context.SvScores
                .Include(x => x.ProfileNavigation)
                .ToListAsync();

            // Get all profiles mapped by their ID
            var profiles = await context.SvProfiles.ToListAsync();
            var profileMap = profiles.ToDictionary(p => p.Id);

            // Group scores by (MusicId, Type) and get the maximum score for each group
            var hiscores = allScores
                .GroupBy(s => new { s.MusicId, s.Type })
                .Select(g => g.OrderByDescending(s => s.Score).FirstOrDefault())
                .Where(s => s != null)
                .ToList();

            // Build the hiscore data
            response.ScoreElement = new HiscoreScoreElement();
            response.ScoreElement.ScoreDataList = new List<HiscoreScoreData>();
            foreach (var score in hiscores)
            {
                if (score?.ProfileNavigation is null || !profileMap.ContainsKey(score.Profile))
                    continue;

                var profile = score.ProfileNavigation;
                var code = profile.Id.ToString("D4");

                response.ScoreElement.ScoreDataList.Add(
                    new ()
                    {
                        Id = (uint)score.Id,
                        Type = (uint)score.Type,
                        AsqSequence = code,
                        ANameId = profile.Name,
                        AScore = (uint)score.Score,
                        LsqSequence = code,
                        LNameId = profile.Name,
                        LScore = (uint)score.Score
                    }
                );
            }

            return response;
        }

    }
}
