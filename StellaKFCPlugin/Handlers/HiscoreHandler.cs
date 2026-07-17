using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Stella.Abstractions.Plugins;
using StellaKFCPlugin.EF;
using StellaKFCPlugin.Models;
using StellaKFCPlugin.Util;

namespace StellaKFCPlugin.Handlers
{
    /// <summary>
    /// Unified <c>hiscore</c> handler (sv6/sv7). Ported from asphyxia
    /// kfc/handlers/features.ts: aggregates per-chart top score/exscore across
    /// all profiles of the requested version and emits an <c>sc.d</c> list.
    /// </summary>
    public class HiscoreHandler : StellaHandler
    {
        [StellaHandler("game", "sv6_hiscore", typeof(HiscoreRequest))]
        public async Task<HiscoreResponse> Hiscore() => await HiscoreInternal(6);

        [StellaHandler("game", "sv7_hiscore", typeof(HiscoreRequest))]
        public async Task<HiscoreResponse> HiscoreNabla() => await HiscoreInternal(7);

        [StellaHandler("game", "hiscore", typeof(HiscoreRequest))]
        public async Task<HiscoreResponse> HiscoreBare() => await HiscoreInternal(Math.Abs(KfcVersion.GetVersion(Model)));

        private async Task<HiscoreResponse> HiscoreInternal(int gameVersion)
        {
            using var db = new StellaKFCContext();
            var response = new HiscoreResponse { ScoreElement = new HiscoreScoreElement() };

            var scores = await db.SvScores
                .Include(s => s.ProfileNavigation)
                .Where(s => s.Version == gameVersion)
                .ToListAsync();

            var profiles = await db.SvProfiles.Where(p => p.Version == gameVersion).ToDictionaryAsync(p => p.Id);

            // Group by (MusicId, Type); pick top score and top exscore (asphyxia features.ts L83-103).
            var grouped = scores.GroupBy(s => new { s.MusicId, s.Type });
            foreach (var g in grouped)
            {
                var rScore = g.OrderByDescending(x => x.Score).First();
                var rExscore = g.OrderByDescending(x => x.Exscore).First();
                if (!profiles.ContainsKey(rScore.Profile)) continue;
                var pScore = profiles[rScore.Profile];
                var pExscore = profiles.ContainsKey(rExscore.Profile) ? profiles[rExscore.Profile] : pScore;

                response.ScoreElement.ScoreDataList.Add(new HiscoreScoreData
                {
                    Id = (uint)rScore.MusicId,
                    Type = (uint)rScore.Type,
                    AsqSequence = KfcVersion.IdToCode(pScore.Id),
                    ANameId = pScore.Name,
                    AScore = (uint)rScore.Score,
                    LsqSequence = KfcVersion.IdToCode(pScore.Id),
                    LNameId = pScore.Name,
                    LScore = (uint)rScore.Score,
                    AxSqSequence = KfcVersion.IdToCode(pExscore.Id),
                    AxNameId = pExscore.Name,
                    AxScore = (uint)rExscore.Exscore,
                    LxSqSequence = KfcVersion.IdToCode(pExscore.Id),
                    LxNameId = pExscore.Name,
                    LxScore = (uint)rExscore.Exscore,
                });
            }
            return response;
        }
    }
}