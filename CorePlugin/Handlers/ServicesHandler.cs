using CorePlugin.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Stella.Abstractions.Plugins;

namespace CorePlugin.Handlers
{
    public class ServicesHandler : StellaHandler
    {
        [StellaHandler("services", "get", typeof(GetServicesRequest))]
        public async Task<GetServicesResponse> GetServices()
        {
            return new GetServicesResponse
            {
                Expire = 600,
                Method = "get",
                Mode = "operation",
                Status = 0,
                Items = new List<ServiceItem>
                {
                    new ServiceItem { Name = "ntp", Url = "ntp://pool.ntp.org/" },
                    new ServiceItem { Name = "keepalive", Url = "http://127.0.0.1/keepalive?pa=127.0.0.1&ia=127.0.0.1&ga=127.0.0.1&ma=127.0.0.1&t1=2&t2=10" },
                    new ServiceItem { Name = "cardmng", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "facility", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "message", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "numbering", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "package", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "pcbevent", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "pcbtracker", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "pkglist", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "posevent", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "userdata", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "userid", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "eacoin", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "dlstatus", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "netlog", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "sidmgr", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "globby", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "local", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "local2", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "lobby", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "lobby2", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "game.sv6_common", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "game.sv6_new", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "game.sv6_load", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "game.sv6_load_m", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "game.sv6_save", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "game.sv6_save_m", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "game.sv6_save_c", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "game.sv6_frozen", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "game.sv6_buy", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "game.sv6_print", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "game.sv6_hiscore", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "game.sv6_load_r", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "game.sv6_save_ap", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "game.sv6_load_ap", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "game.sv6_lounge", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "game.sv6_shop", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "game.sv6_save_e", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "game.sv6_save_mega", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "game.sv6_play_e", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "game.sv6_play_s", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "game.sv6_entry_s", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "game.sv6_entry_e", Url = "http://localhost:80/eamuse" },
                    new ServiceItem { Name = "game.sv6_exception", Url = "http://localhost:80/eamuse" },
                }
            };
        }
    }
}

