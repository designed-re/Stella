using CorePlugin.Models;
using Microsoft.Extensions.DependencyInjection;
using Stella.Abstractions.Plugins;
using Stella.Abstractions.Configuration;
using Stella.Services;

namespace CorePlugin.Handlers
{
    public class ServicesHandler : StellaHandler
    {
        // Base URL for all e-amusement service endpoints. Sourced from the
        // "Stella:ServerUrl" key in appsettings.json (see StellaOptions).
        private static string BaseUrl => StellaOptions.ServerUrl;

        // Keepalive endpoint. Sourced from "Stella:KeepaliveUrl" / "Stella:ServerHost".
        private static string KeepaliveUrl => StellaOptions.ResolvedKeepaliveUrl;

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

            // Dynamically discover services from loaded plugins. PluginService
            // stores keys as "service:module" (e.g. "game:sv6_common", "iidx:...").
            // e-amusement only sends service-level names, so advertise every unique
            // service prefix that has at least one registered handler and is not
            // already in the core list above. This makes adding a new game plugin
            // (any service prefix, not just "game*") a drop-in: its routes are
            // advertised automatically with no host edit.
            var pluginService = HttpContext.RequestServices.GetRequiredService<PluginService>();
            var known = new HashSet<string>(items.Select(i => i.Name), StringComparer.Ordinal);
            foreach (var key in pluginService.RegisteredHandlers)
            {
                var serviceName = key.Split(':')[0];
                if (!string.IsNullOrEmpty(serviceName) && known.Add(serviceName))
                    items.Add(new ServiceItem { Name = serviceName, Url = url });
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
