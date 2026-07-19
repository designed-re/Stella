using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stella.Abstractions;
using Stella.Abstractions.Plugins;
using StellaKFCPlugin.EF;
using StellaKFCPlugin.Models;
using StellaKFCPlugin.Util;

namespace StellaKFCPlugin.Handlers
{
    /// <summary>
    /// Unified stub handlers for sv6/sv7: save_mega, exception, log, entry_e.
    /// Also implements buy and print (ported from asphyxia profiles.ts).
    /// </summary>
    public class MiscKfcHandler : StellaHandler
    {
        // save_mega — asphyxia stub (true)
        [StellaHandler("game", "sv6_save_mega", typeof(StubRequest))]
        public async Task<StubResponse> SaveMega() => new();

        [StellaHandler("game", "sv7_save_mega", typeof(StubRequest))]
        public async Task<StubResponse> SaveMegaNabla() => new();

        [StellaHandler("game", "save_mega", typeof(StubRequest))]
        public async Task<StubResponse> SaveMegaBare() => new();

        [StellaHandler("game_3", "save_mega", typeof(StubRequest))]
        public async Task<StubResponse> SaveMegaBareGame3() => new();


        // exception — asphyxia stub (true)
        [StellaHandler("game", "sv6_exception", typeof(StubRequest))]
        public async Task<StubResponse> Exception() => new();

        [StellaHandler("game", "sv7_exception", typeof(StubRequest))]
        public async Task<StubResponse> ExceptionNabla() => new();

        [StellaHandler("game", "exception", typeof(StubRequest))]
        public async Task<StubResponse> ExceptionBare() => new();
        [StellaHandler("game_3", "exception", typeof(StubRequest))]
        public async Task<StubResponse> ExceptionBareGame3() => new();

        // log — asphyxia send.success()
        [StellaHandler("game", "sv6_log", typeof(StubRequest))]
        public async Task<StubResponse> Log() => new();

        [StellaHandler("game", "sv7_log", typeof(StubRequest))]
        public async Task<StubResponse> LogNabla() => new();

        [StellaHandler("game", "log", typeof(StubRequest))]
        public async Task<StubResponse> LogBare() => new();
        [StellaHandler("game_3", "log", typeof(StubRequest))]
        public async Task<StubResponse> LogBareGame3() => new();

        // entry_e — asphyxia logs eid, send.success()
        [StellaHandler("game", "sv6_entry_e", typeof(EntryERequest))]
        public async Task<StubResponse> EntryE() => new();

        [StellaHandler("game", "sv7_entry_e", typeof(EntryERequest))]
        public async Task<StubResponse> EntryENabla() => new();

        [StellaHandler("game", "entry_e", typeof(EntryERequest))]
        public async Task<StubResponse> EntryEBare() => new();
        [StellaHandler("game_3", "entry_e", typeof(EntryERequest))]
        public async Task<StubResponse> EntryEBareGame3() => new();

        // buy — asphyxia profiles.ts buy
        [StellaHandler("game", "sv6_buy", typeof(BuyRequest))]
        public async Task<BuyResponse> Buy() => await BuyInternal(6);

        [StellaHandler("game", "sv7_buy", typeof(BuyRequest))]
        public async Task<BuyResponse> BuyNabla() => await BuyInternal(7);

        [StellaHandler("game", "buy", typeof(BuyRequest))]
        public async Task<BuyResponse> BuyBare() => await BuyInternal(Math.Abs(KfcVersion.GetVersion(Model)));
        [StellaHandler("game_3", "buy", typeof(BuyRequest))]
        public async Task<BuyResponse> BuyBareGame3() => await BuyInternal(Math.Abs(KfcVersion.GetVersion(Model)));

        // print — asphyxia profiles.ts print
        [StellaHandler("game", "sv6_print", typeof(PrintRequest))]
        public async Task<PrintResponse> Print() => await PrintInternal(6);

        [StellaHandler("game", "sv7_print", typeof(PrintRequest))]
        public async Task<PrintResponse> PrintNabla() => await PrintInternal(7);

        [StellaHandler("game", "print", typeof(PrintRequest))]
        public async Task<PrintResponse> PrintBare() => await PrintInternal(Math.Abs(KfcVersion.GetVersion(Model)));
        [StellaHandler("game_3", "print", typeof(PrintRequest))]
        public async Task<PrintResponse> PrintBareGame3() => await PrintInternal(Math.Abs(KfcVersion.GetVersion(Model)));

        private async Task<BuyResponse> BuyInternal(int gameVersion)
        {
            var request = Request as BuyRequest;
            if (request is null) return new BuyResponse { Status = "1" };
            using var db = new StellaKFCContext();

            var profile = await db.SvProfiles.SingleOrDefaultAsync(x => x.RefId == request.RefId && x.Version == gameVersion);
            if (profile is null) return new BuyResponse { Status = "1" };

            // asphyxia buy (profiles.ts L1130-1162): the game sends <item> with
            // FLAT __count arrays (item.item_type / item.item_id / item.param /
            // item.price), NOT <item><info>...</info></item>. asphyxia zips the
            // four arrays with _.zipWith. PreprocessXmlForArrays expands each
            // __count array into repeated elements, so List<int>/List<uint>
            // collects them in order. cost = sum of all prices.
            var itemTypes = request.ItemElement.ItemTypes;
            var itemIds = request.ItemElement.ItemIds;
            var itemParams = request.ItemElement.Params;
            var prices = request.ItemElement.Prices;
            int count = new[] { itemTypes.Count, itemIds.Count, itemParams.Count, prices.Count }.Min();
            int cost = prices.Sum();
            int earnedBlocks = request.EarnedGamecoinBlock;
            int earnedPackets = request.EarnedGamecoinPacket;

            if (request.CurrencyType)
            {
                // blocks
                long change = (long)earnedBlocks - cost;
                if ((long)profile.Blocks + change >= 0)
                    profile.Blocks = (uint)((long)profile.Blocks + change);
            }
            else
            {
                // packets
                long change = (long)earnedPackets - cost;
                if ((long)profile.Packets + change >= 0)
                    profile.Packets = (uint)((long)profile.Packets + change);
            }

            // Save items
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    var itemType = itemTypes[i];
                    var itemId = itemIds[i];
                    var itemParam = itemParams[i];
                    var rec = await db.SvItems.AsNoTracking().SingleOrDefaultAsync(x =>
                        x.Profile == profile.Id && x.ItemId == itemId && x.Type == (byte)itemType && x.Version == gameVersion);
                    if (rec is null)
                        await db.SvItems.AddAsync(new SvItem { ItemId = itemId, Param = itemParam, Type = (byte)itemType, Profile = profile.Id, Version = gameVersion });
                    else
                        db.SvItems.Update(new SvItem { Id = rec.Id, ItemId = itemId, Param = itemParam, Type = (byte)itemType, Profile = profile.Id, Version = gameVersion });
                }
            }

            await db.SaveChangesAsync();

            return new BuyResponse
            {
                GamecoinPacket = profile.Packets,
                GamecoinBlock = profile.Blocks,
            };
        }

        private async Task<PrintResponse> PrintInternal(int gameVersion)
        {
            var request = Request as PrintRequest;
            if (request is null) return new PrintResponse { Status = "1" };

            // asphyxia print: returns genesis_cards + after_power (generator_id list)
            var response = new PrintResponse { Result = 0 };
            if (request.GenesisCards != null)
            {
                foreach (var gc in request.GenesisCards)
                {
                    response.GenesisCards.Add(new PrintGenesisCard { Index = gc.Index, PrintId = gc.PrintId });
                }

                // after_power: unique generator_ids with param=10
                var generatorIds = request.GenesisCards
                    .Select(g => g.GeneratorId)
                    .Distinct()
                    .ToList();
                foreach (var gid in generatorIds)
                {
                    response.AfterPower.Add(new PrintAfterPower { GeneratorId = gid, Param = 10 });
                }
            }
            return response;
        }
    }

    // Stub request/response (for handlers that just return success)
    [XmlRoot(ElementName = "game")]
    public class StubRequest : IStellaEAmuseRequest { }

    [XmlRoot(ElementName = "game")]
    public class StubResponse : IStellaEAmuseResponse
    {
        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; } = "0";
    }

    // entry_e
    [XmlRoot(ElementName = "game")]
    public class EntryERequest : IStellaEAmuseRequest
    {
        [XmlElement(ElementName = "eid")]
        public int Eid { get; set; }
    }

    // buy
    [XmlRoot(ElementName = "game")]
    public class BuyRequest : IStellaEAmuseRequest
    {
        [XmlElement(ElementName = "refid")]
        public string RefId { get; set; }

        [XmlElement(ElementName = "currency_type")]
        public bool CurrencyType { get; set; }

        [XmlElement(ElementName = "earned_gamecoin_packet")]
        public int EarnedGamecoinPacket { get; set; }

        [XmlElement(ElementName = "earned_gamecoin_block")]
        public int EarnedGamecoinBlock { get; set; }

        [XmlElement(ElementName = "item")]
        public BuyItemElement ItemElement { get; set; } = new();
    }

    public class BuyItemElement
    {
        [XmlElement(ElementName = "item_type")]
        public List<int> ItemTypes { get; set; } = new();

        [XmlElement(ElementName = "item_id")]
        public List<uint> ItemIds { get; set; } = new();

        [XmlElement(ElementName = "param")]
        public List<uint> Params { get; set; } = new();

        [XmlElement(ElementName = "price")]
        public List<int> Prices { get; set; } = new();
    }

    [XmlRoot(ElementName = "game")]
    public class BuyResponse : IStellaEAmuseResponse
    {
        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; } = "0";

        [XmlElement(ElementName = "gamecoin_packet")]
        public uint GamecoinPacket { get; set; }

        [XmlElement(ElementName = "gamecoin_block")]
        public uint GamecoinBlock { get; set; }
    }

    // print
    [XmlRoot(ElementName = "game")]
    public class PrintRequest : IStellaEAmuseRequest
    {
        [XmlElement(ElementName = "genesis_card")]
        public List<PrintGenesisCardRequest> GenesisCards { get; set; } = new();
    }

    public class PrintGenesisCardRequest
    {
        [XmlElement(ElementName = "index")]
        public int Index { get; set; }

        [XmlElement(ElementName = "print_id")]
        public int PrintId { get; set; }

        [XmlElement(ElementName = "generator_id")]
        public int GeneratorId { get; set; }
    }

    [XmlRoot(ElementName = "game")]
    public class PrintResponse : IStellaEAmuseResponse
    {
        [XmlAttribute(AttributeName = "status")]
        public string Status { get; set; } = "0";

        [XmlElement(ElementName = "result")]
        public sbyte Result { get; set; }

        // asphyxia print: `genesis_cards: [...]` and `after_power: [...]` are
        // direct arrays -> KBinJSON serialises them as repeated same-named
        // elements (<genesis_cards>...</genesis_cards> x N), NOT wrapped in an
        // <info> child. XmlElement (not XmlArray) reproduces that wire shape.
        [XmlElement(ElementName = "genesis_cards")]
        public List<PrintGenesisCard> GenesisCards { get; set; } = new();

        [XmlElement(ElementName = "after_power")]
        public List<PrintAfterPower> AfterPower { get; set; } = new();
    }

    public class PrintGenesisCard
    {
        [XmlElement(ElementName = "index")]
        public int Index { get; set; }

        [XmlElement(ElementName = "print_id")]
        public int PrintId { get; set; }
    }

    public class PrintAfterPower
    {
        [XmlElement(ElementName = "generator_id")]
        public int GeneratorId { get; set; }

        [XmlElement(ElementName = "param")]
        public int Param { get; set; }
    }
}
