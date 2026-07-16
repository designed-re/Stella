using CorePlugin.Models;
using Stella.Abstractions.Plugins;

namespace CorePlugin.Handlers
{
    public class PcbEventHandler : StellaHandler
    {
        [StellaHandler("pcbevent", "put", typeof(GetPcbEventRequest))]
        public async Task<GetPcbEventResponse> GetPcbTrackerAlive()
        {
            return new GetPcbEventResponse(){};
        }
    }
}

