using CorePlugin.Models;
using Newtonsoft.Json;
using Stella.Abstractions.Plugins;

namespace CorePlugin.Handlers
{
    public class PcbEventHandler : StellaHandler
    {
        [StellaHandler("pcbevent", "put", typeof(GetPcbEventRequest))]
        public async Task<GetPcbEventResponse> GetPcbTrackerAlive()
        {
            Console.WriteLine(JsonConvert.SerializeObject(Request as GetPcbEventRequest));
            return new GetPcbEventResponse(){};
        }
    }
}

