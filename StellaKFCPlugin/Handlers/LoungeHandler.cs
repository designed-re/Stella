using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stella.Abstractions.Plugins;
using StellaKFCPlugin.Models;
using StellaKFCPlugin.Util;

namespace StellaKFCPlugin.Handlers
{
    /// <summary>
    /// Unified <c>lounge</c>/<c>play_e</c>/<c>play_s</c>/<c>entry_s</c>/<c>entry_e</c>/<c>shop</c>
    /// handlers. The lounge/entry_s matchmaker logic is ported from asphyxia
    /// kfc/handlers/features.ts; room state is kept in-process (mirrors asphyxia).
    /// </summary>
    public class LoungeHandler : StellaHandler
    {
        internal sealed class MatchRoom
        {
            public int Version;
            public int CVer;
            public int Filter;
            public int Mid;
            public int PRest;
            public int PNum;
            public int Sec;
            public List<MatchPlayer> Players = new();
        }

        internal sealed class MatchPlayer
        {
            public int[] Gip = Array.Empty<int>();
            public int[] Lip = Array.Empty<int>();
            public int Port;
        }

        internal static readonly ConcurrentBag<MatchRoom> MatchRooms = new();

        [StellaHandler("game", "sv6_lounge", typeof(LoungeRequest))]
        public async Task<LoungeResponse> Lounge() => new() { Interval = 30 };

        [StellaHandler("game", "sv7_lounge", typeof(LoungeRequest))]
        public async Task<LoungeResponse> LoungeNabla() => new() { Interval = 30 };
    }
}