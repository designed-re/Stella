using CorePlugin.Models;
using Microsoft.Extensions.DependencyInjection;
using Stella.Abstractions.Plugins;
using Stella.Services;

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

            // Core services that the game expects. These are listed at the
            // service level (not method level) because that's what e-amusement
            // uses in the services.get response. Some have no backing handler
            // (e.g. numbering, lobby) but must still be advertised.
            var items = new List<ServiceItem>
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
            };

            // Dynamically discover game services from loaded plugins.
            // PluginService stores keys as "service:module" (e.g. "game:sv6_common").
            // Extract unique service-level names (e.g. "game", "game_3") —
            // the e-amusement protocol only sends service names, not methods.
            var pluginService = HttpContext.RequestServices.GetRequiredService<PluginService>();
            var gameServiceNames = new HashSet<string>();
            foreach (var key in pluginService.RegisteredHandlers)
            {
                if (key.StartsWith("game"))
                {
                    var serviceName = key.Split(':')[0];
                    gameServiceNames.Add(serviceName);
                }
            }
            foreach (var name in gameServiceNames)
            {
                items.Add(new ServiceItem { Name = name, Url = url });
            }

            return new GetServicesResponse
            {
                Expire = 600,
                Method = "get",
                Mode = "operation",
                Status = 0,
                Items = items,
            };
        }
    }
}
