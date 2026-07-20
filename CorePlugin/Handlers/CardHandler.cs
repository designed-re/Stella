using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using CorePlugin.EF;
using CorePlugin.Models;
using Stella.Abstractions;
using Stella.Abstractions.Plugins;

namespace CorePlugin.Handlers
{
    public class CardHandler : StellaHandler
    {
        [StellaHandler("cardmng", "inquire", typeof(CardInquireRequest))]
        public async Task<CardInquireResponse> CardInquire()
        {
            var request = Request as CardInquireRequest;

            using var context = new CoreContext();
            var card = context.Cards.FirstOrDefault(x => x.CardId == request.Cardid);

            if (card is null)
            {
                return new CardInquireResponse() { Status = "112" };
            }

            // asphyxia cardmng.inquire: binded = CheckProfile(gameCode, refid) ? 1 : 0.
            // The game code is the first colon-delimited segment of the model
            // string (e.g. "KFC:J:A:A:..."). We query the matching game plugin
            // through the shared registry so CorePlugin needs no direct reference
            // to game-plugin data stores.
            string gameCode = Model?.Split(':').FirstOrDefault() ?? string.Empty;
            var gamePlugin = StellaPluginRegistry.GetByGameCode(gameCode);
            bool binded = gamePlugin is not null && await gamePlugin.ProfileExistsAsync(card.RefId);

            return new CardInquireResponse()
            {
                Binded = binded ? 1 : 0,
                Dataid = card.RefId,
                Ecflag = 1,
                Newflag = 0,
                Expired = 0,
                Refid = card.RefId,
                Status = "0"
            };
        }

        [StellaHandler("cardmng", "authpass", typeof(CardAuthpassRequest))]
        public async Task<CardAuthpassResponse> Authpass()
        {
            var request = Request as CardAuthpassRequest;

            using var context = new CoreContext();
            var card = context.Cards.FirstOrDefault(x => x.RefId == request.RefId.ToUpper());

            int status;
            if (card != null && card.PassCode == request.Pass)
            {
                status = 0;
            }
            else
            {
                status = 116;
            }

            return new CardAuthpassResponse()
            {
                Status = status.ToString()
            };
        }

        [StellaHandler("cardmng", "getrefid", typeof(CardGetRefIdRequest))]
        public async Task<CardGetRefIdResponse> GetRefId()
        {
            var request = Request as CardGetRefIdRequest;
            using var context = new CoreContext();

            string cardId = request.CardId.ToUpper();
            string passwd = request.Passwd;

            // Card already exists — asphyxia updates the pin and re-binds, then
            // returns the existing refid/dataid (profiles.ts cardmng.getrefid).
            var existing = context.Cards.FirstOrDefault(c => c.CardId == cardId);
            if (existing is not null)
            {
                existing.PassCode = passwd;
                await context.SaveChangesAsync();
                return new CardGetRefIdResponse { DataId = existing.RefId, RefId = existing.RefId };
            }

            // Generate random RefId and DataId (same value)
            byte[] refIdBytes = new byte[8];
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                rng.GetBytes(refIdBytes);
            }
            
            // Convert bytes to hex string
            string refIdHex = BitConverter.ToString(refIdBytes).Replace("-", "").ToUpper();

            // Create new card
            var card = new Card()
            {
                CardId = cardId,
                RefId = refIdHex,
                CardNo = cardId,
                PassCode = passwd,
                Paseli = 10000
            };

            context.Cards.Add(card);
            await context.SaveChangesAsync();

            return new CardGetRefIdResponse()
            {
                DataId = card.RefId,
                RefId = card.RefId
            };
        }

        [StellaHandler("cardmng", "bindmodel", typeof(CardBindModelRequest))]
        public async Task<CardBindModelResponse> BindModel()
        {
            var request = Request as CardBindModelRequest;
            // asphyxia just returns dataid = refid (no real binding logic).
            return new CardBindModelResponse { DataId = request?.RefId ?? "DEADC0DEFEEDBEEF" };
        }
    }
}


