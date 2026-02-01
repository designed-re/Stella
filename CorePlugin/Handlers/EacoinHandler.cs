using System.Linq;
using System.Threading.Tasks;
using CorePlugin.EF;
using CorePlugin.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stella.Abstractions;
using Stella.Abstractions.Plugins;

namespace CorePlugin.Handlers
{
    public class EacoinHandler : StellaHandler
    {
        [StellaHandler("eacoin", "checkin", typeof(CheckInRequest))]
        public async Task<CheckInResponse> CheckIn()
        {
            var request = Request as CheckInRequest;
            var context = new CoreContext();

            try
            {
                // Find card by ID
                var card = await context.Cards.SingleOrDefaultAsync(x => x.CardId == request.CardId);

                if (card == null)
                {
                    Logger.LogWarning($"Card not found: {request.CardId}");
                    return new CheckInResponse
                    {
                        Status = "1",
                        Balance = 0,
                        SessionId = "",
                        AcStatus = 1,
                        Sequence = 0,
                        AcId = "",
                        AcName = ""
                    };
                }

                // Generate session ID
                string session = string.Concat(System.Guid.NewGuid().ToString("N").ToLower().Take(16));
                card.PaseliSession = session;
                await context.SaveChangesAsync();
                return new CheckInResponse
                {
                    Balance = card.Paseli,
                    SessionId = session,
                    AcStatus = 0,
                    Sequence = 1,
                    AcId = session,
                    AcName = session
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in eacoin.checkin");
                throw new StellaHandlerException(500);
            }
            finally
            {
                context.Dispose();
            }
        }

        [StellaHandler("eacoin", "consume", typeof(ConsumeRequest))]
        public async Task<ConsumeResponse> Consume()
        {
            var request = Request as ConsumeRequest;
            var context = new CoreContext();

            try
            {
                // Find card by session ID
                var card = await context.Cards.SingleOrDefaultAsync(x => x.PaseliSession == request.SessionId);

                int balance = card?.Paseli ?? 0;

                // Insufficient balance or card not found
                if (balance < request.Payment || card is null)
                {
                    Logger.LogWarning($"Insufficient balance or card not found. Balance: {balance}, Payment: {request.Payment}");
                    return new ConsumeResponse
                    {
                        Balance = balance,
                        Autocharge = 0,
                        AcStatus = 1  // Error status
                    };
                }

                // Deduct payment
                card.Paseli -= request.Payment;
                await context.SaveChangesAsync();

                return new ConsumeResponse
                {
                    Balance = card.Paseli,
                    Autocharge = 0,
                    AcStatus = 0  // Success status
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in eacoin.consume");
                throw new StellaHandlerException(500);
            }
            finally
            {
                context.Dispose();
            }
        }
    }
}
