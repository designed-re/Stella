using System;
using System.Collections.Generic;
using System.Text;
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

            var context = new CoreContext();
            var card = context.Cards.FirstOrDefault(x=> x.CardId == request.Cardid);

            if (card is null)
            {
                return new CardInquireResponse(){Status = "112"};
            }
            else
            {
                return new CardInquireResponse()
                {
                    Binded = 1,
                    Dataid = card.RefId,
                    Ecflag = 1,
                    Newflag = 0,
                    Expired = 0,
                    Refid = card.RefId,
                    Status = "0"
                };
            }
        }

        [StellaHandler("cardmng", "authpass", typeof(CardAuthpassRequest))]
        public async Task<CardAuthpassResponse> Authpass()
        {
            var request = Request as CardAuthpassRequest;

            var context = new CoreContext();
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
            var context = new CoreContext();

            string cardId = request.CardId.ToUpper();
            string passwd = request.Passwd;

            // Check if card already exists
            if (context.Cards.Any(c => c.CardId == cardId))
            {
                return new CardGetRefIdResponse();
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
    }
}


