using CorePlugin.Models;
using Stella.Abstractions.Plugins;

namespace CorePlugin.Handlers
{
    public class PcbTrackerHandler : StellaHandler
    {
        [StellaHandler("pcbtracker", "alive", typeof(GetPcbTrackerRequest))]
        public async Task<GetPcbTrackerResponse> GetPcbTrackerAlive()
        {
            Console.WriteLine((Request as GetPcbTrackerRequest).Accountid);
            return new GetPcbTrackerResponse()
            {
                Expire = 600,
                ECEnable = true,
                ECLimit = 0,
                Limit = 0,
                Time = DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000
            };
        }
    }
}

