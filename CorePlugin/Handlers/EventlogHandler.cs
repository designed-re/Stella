using System.Threading.Tasks;
using CorePlugin.Models;
using Microsoft.Extensions.Logging;
using Stella.Abstractions;
using Stella.Abstractions.Plugins;

namespace CorePlugin.Handlers
{
    public class EventlogHandler : StellaHandler
    {
        [StellaHandler("eventlog", "write", typeof(EventlogRequest))]
        public async Task<EventlogResponse> Write()
        {
            try
            {
                var request = Request as EventlogRequest;

                // Log the event
                Logger.LogInformation($"Eventlog write received");

                // Return success response
                return new EventlogResponse
                {
                    Status = 0,
                    GameSession = 1,
                    LogErrLevel = 0,
                    LogSendFlg = 0,
                    EvtIdNoSendFlg = 0
                };
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error in eventlog.write");
                throw new StellaHandlerException(500);
            }
        }
    }
}
