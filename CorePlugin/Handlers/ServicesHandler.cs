using CorePlugin.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Stella.Abstractions.Plugins;

namespace CorePlugin.Handlers
{
    public class ServicesHandler : StellaHandler
    {
        // Base URL for all e-amusement service endpoints. Override with the
        // STELLA_SERVER_URL env var (e.g. "http://10.0.1.133:8080/eamuse").
        // Defaults to localhost:80 for backwards compatibility.
        private static string BaseUrl =>
            Environment.GetEnvironmentVariable("STELLA_SERVER_URL") ?? "http://localhost:80/eamuse";

        // Keepalive endpoint. Override with STELLA_KEEPALIVE_URL.
        private static string KeepaliveUrl =>
            Environment.GetEnvironmentVariable("STELLA_KEEPALIVE_URL")
            ?? $"http://{(Environment.GetEnvironmentVariable("STELLA_SERVER_HOST") ?? "127.0.0.1")}/keepalive?pa=127.0.0.1&ia=127.0.0.1&ga=127.0.0.1&ma=127.0.0.1&t1=2&t2=10";

        [StellaHandler("services", "get", typeof(GetServicesRequest))]
        public async Task<GetServicesResponse> GetServices()
        {
            var url = BaseUrl;
            return new GetServicesResponse
            {
                Expire = 600,
                Method = "get",
                Mode = "operation",
                Status = 0,
                Items = new List<ServiceItem>
                {
                    new ServiceItem { Name = "ntp", Url = "ntp://pool.ntp.org/" },
                    new ServiceItem { Name = "keepalive", Url = KeepaliveUrl },
                    new ServiceItem { Name = "cardmng", Url = url },
                    new ServiceItem { Name = "facility", Url = url },
                    new ServiceItem { Name = "message", Url = url },
                    new ServiceItem { Name = "numbering", Url = url },
                    new ServiceItem { Name = "package", Url = url },
                    new ServiceItem { Name = "pcbevent", Url = url },
                    new ServiceItem { Name = "pcbtracker", Url = url },
                    new ServiceItem { Name = "pkglist", Url = url },
                    new ServiceItem { Name = "posevent", Url = url },
                    new ServiceItem { Name = "userdata", Url = url },
                    new ServiceItem { Name = "userid", Url = url },
                    new ServiceItem { Name = "eacoin", Url = url },
                    new ServiceItem { Name = "dlstatus", Url = url },
                    new ServiceItem { Name = "netlog", Url = url },
                    new ServiceItem { Name = "sidmgr", Url = url },
                    new ServiceItem { Name = "globby", Url = url },
                    new ServiceItem { Name = "local", Url = url },
                    new ServiceItem { Name = "local2", Url = url },
                    new ServiceItem { Name = "lobby", Url = url },
                    new ServiceItem { Name = "lobby2", Url = url },
                    // EXCEED GEAR (sv6)
                    new ServiceItem { Name = "game.sv6_common", Url = url },
                    new ServiceItem { Name = "game.sv6_new", Url = url },
                    new ServiceItem { Name = "game.sv6_load", Url = url },
                    new ServiceItem { Name = "game.sv6_load_m", Url = url },
                    new ServiceItem { Name = "game.sv6_save", Url = url },
                    new ServiceItem { Name = "game.sv6_save_m", Url = url },
                    new ServiceItem { Name = "game.sv6_save_c", Url = url },
                    new ServiceItem { Name = "game.sv6_save_pb", Url = url },
                    new ServiceItem { Name = "game.sv6_save_valgene", Url = url },
                    new ServiceItem { Name = "game.sv6_frozen", Url = url },
                    new ServiceItem { Name = "game.sv6_buy", Url = url },
                    new ServiceItem { Name = "game.sv6_print", Url = url },
                    new ServiceItem { Name = "game.sv6_hiscore", Url = url },
                    new ServiceItem { Name = "game.sv6_load_r", Url = url },
                    new ServiceItem { Name = "game.sv6_lounge", Url = url },
                    new ServiceItem { Name = "game.sv6_shop", Url = url },
                    new ServiceItem { Name = "game.sv6_save_e", Url = url },
                    new ServiceItem { Name = "game.sv6_save_mega", Url = url },
                    new ServiceItem { Name = "game.sv6_play_e", Url = url },
                    new ServiceItem { Name = "game.sv6_play_s", Url = url },
                    new ServiceItem { Name = "game.sv6_entry_s", Url = url },
                    new ServiceItem { Name = "game.sv6_entry_e", Url = url },
                    new ServiceItem { Name = "game.sv6_exception", Url = url },
                    // NABLA (sv7)
                    new ServiceItem { Name = "game.sv7_common", Url = url },
                    new ServiceItem { Name = "game.sv7_new", Url = url },
                    new ServiceItem { Name = "game.sv7_load", Url = url },
                    new ServiceItem { Name = "game.sv7_load_m", Url = url },
                    new ServiceItem { Name = "game.sv7_save", Url = url },
                    new ServiceItem { Name = "game.sv7_save_m", Url = url },
                    new ServiceItem { Name = "game.sv7_save_c", Url = url },
                    new ServiceItem { Name = "game.sv7_save_pb", Url = url },
                    new ServiceItem { Name = "game.sv7_save_valgene", Url = url },
                    new ServiceItem { Name = "game.sv7_frozen", Url = url },
                    new ServiceItem { Name = "game.sv7_hiscore", Url = url },
                    new ServiceItem { Name = "game.sv7_load_r", Url = url },
                    new ServiceItem { Name = "game.sv7_lounge", Url = url },
                    new ServiceItem { Name = "game.sv7_save_e", Url = url },
                    new ServiceItem { Name = "game.sv7_play_e", Url = url },
                    new ServiceItem { Name = "game.sv7_play_s", Url = url },
                }
            };
        }
    }
}