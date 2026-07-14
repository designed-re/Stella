using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Stella.Abstractions;
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
            public List<int> Gip = new();
            public List<int> Lip = new();
            public int Port;
        }

        internal static readonly List<MatchRoom> MatchRooms = new();
        internal static readonly object _lock = new();

        [StellaHandler("game", "sv6_lounge", typeof(LoungeRequest))]
        public async Task<LoungeResponse> Lounge() => await LoungeInternal(6);

        [StellaHandler("game", "sv7_lounge", typeof(LoungeRequest))]
        public async Task<LoungeResponse> LoungeNabla() => await LoungeInternal(7);

        [StellaHandler("game", "sv6_entry_s", typeof(EntrySRequest))]
        public async Task<EntrySResponse> EntryS() => await EntrySInternal(6);

        [StellaHandler("game", "sv7_entry_s", typeof(EntrySRequest))]
        public async Task<EntrySResponse> EntrySNabla() => await EntrySInternal(7);

        private Task<LoungeResponse> LoungeInternal(int gameVersion)
        {
            var request = Request as LoungeRequest;
            var filter = request?.Filter ?? 0;

            lock (_lock)
            {
                var matches = MatchRooms.Where(r => r.Version == gameVersion && r.Filter == filter).ToList();
                if (matches.Count < 1)
                    return Task.FromResult(new LoungeResponse { Interval = 5 });

                var longestWait = matches.Max(m => m.Sec);
                return Task.FromResult(new LoungeResponse { Interval = 10, Wait = (uint)longestWait });
            }
        }

        private Task<EntrySResponse> EntrySInternal(int gameVersion)
        {
            var request = Request as EntrySRequest;
            if (request is null) return Task.FromResult(new EntrySResponse { Status = "1" });

            lock (_lock)
            {
                var lipStr = string.Join(".", request.Lip);
                var gipStr = string.Join(".", request.Gip);

                // Check if player already in a matching room
                MatchRoom? existingRoom = null;
                int existingPlayIdx = -1;
                foreach (var room in MatchRooms)
                {
                    if (room.Version == gameVersion && room.CVer == request.CVer &&
                        room.Filter == request.Filter && room.Mid == request.Mid)
                    {
                        var idx = room.Players.FindIndex(p => string.Join(".", p.Lip) == lipStr);
                        if (idx != -1)
                        {
                            existingRoom = room;
                            existingPlayIdx = idx;
                            break;
                        }
                    }
                }

                if (existingRoom != null)
                {
                    // Already in room — re-send opponent data
                    var others = existingRoom.Players.ToList();
                    others.RemoveAt(existingPlayIdx);
                    return Task.FromResult(BuildEntryResponse(request.EntryId, others));
                }

                // Find room with a free slot
                foreach (var room in MatchRooms)
                {
                    if (room.Version == gameVersion && room.CVer == request.CVer &&
                        room.Filter == request.Filter && room.Mid == request.Mid)
                    {
                        if (room.Players.Count < room.PRest + room.PNum)
                        {
                            room.Players.Add(new MatchPlayer { Gip = request.Gip.ToList(), Lip = request.Lip.ToList(), Port = request.Port });
                            var others = room.Players.ToList();
                            others.RemoveAt(room.Players.Count - 1);
                            return Task.FromResult(BuildEntryResponse(request.EntryId, others));
                        }
                    }
                }

                // No room found — create new
                var newRoom = new MatchRoom
                {
                    Version = gameVersion,
                    CVer = request.CVer,
                    Filter = request.Filter,
                    Mid = request.Mid,
                    PRest = request.PRest,
                    PNum = request.PNum,
                    Sec = request.Sec,
                    Players = new List<MatchPlayer> { new() { Gip = request.Gip.ToList(), Lip = request.Lip.ToList(), Port = request.Port } },
                };
                MatchRooms.Add(newRoom);

                // Schedule room deletion after sec seconds
                var sec = request.Sec;
                _ = Task.Run(async () =>
                {
                    await Task.Delay(sec * 1000);
                    lock (_lock)
                    {
                        var idx = MatchRooms.FindIndex(r =>
                            r.Players.Count > 0 && string.Join(".", r.Players[0].Lip) == lipStr);
                        if (idx >= 0) MatchRooms.RemoveAt(idx);
                    }
                });

                // New room, no opponents
                return Task.FromResult(new EntrySResponse { EntryId = (uint)request.EntryId });
            }
        }

        private static EntrySResponse BuildEntryResponse(int entryId, List<MatchPlayer> others)
        {
            var resp = new EntrySResponse { EntryId = (uint)entryId };
            foreach (var p in others)
            {
                resp.Entries.Add(new EntryPlayer { Port = (ushort)p.Port, Gip = p.Gip, Lip = p.Lip });
            }
            return resp;
        }
    }

    // entry_s request/response
    [XmlRoot(ElementName = "game")]
    public class EntrySRequest : IStellaEAmuseRequest
    {
        [XmlElement(ElementName = "c_ver")] public int CVer { get; set; }
        [XmlElement(ElementName = "p_num")] public int PNum { get; set; }
        [XmlElement(ElementName = "p_rest")] public int PRest { get; set; }
        [XmlElement(ElementName = "filter")] public int Filter { get; set; }
        [XmlElement(ElementName = "mid")] public int Mid { get; set; }
        [XmlElement(ElementName = "sec")] public int Sec { get; set; }
        [XmlElement(ElementName = "claim")] public int Claim { get; set; }
        [XmlElement(ElementName = "entry_id")] public int EntryId { get; set; }
        [XmlElement(ElementName = "port")] public int Port { get; set; }
        [XmlElement(ElementName = "gip")] public string GipRaw { get; set; } = "";
        [XmlElement(ElementName = "lip")] public string LipRaw { get; set; } = "";

        [XmlIgnore]
        public List<int> Gip => GipRaw.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();

        [XmlIgnore]
        public List<int> Lip => LipRaw.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList();
    }

    [XmlRoot(ElementName = "game")]
    public class EntrySResponse : IStellaEAmuseResponse
    {
        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; } = "0";

        [XmlElement(ElementName = "entry_id")]
        public uint EntryId { get; set; }

        [XmlElement(ElementName = "entry")]
        public List<EntryPlayer> Entries { get; set; } = new();
    }

    public class EntryPlayer
    {
        [XmlElement(ElementName = "port")]
        public ushort Port { get; set; }

        [XmlElement(ElementName = "gip")]
        public List<int> Gip { get; set; } = new();

        [XmlElement(ElementName = "lip")]
        public List<int> Lip { get; set; } = new();
    }
}