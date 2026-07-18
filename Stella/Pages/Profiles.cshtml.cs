using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stella.Abstractions.Cards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Stella.Abstractions.Plugins;
using Stella.Services;

namespace Stella.WebUI.Pages;

[Authorize]
public class ProfilesModel : PageModel
{
    private readonly PluginService _plugins;
    public ProfilesModel(PluginService plugins) => _plugins = plugins;

    public IReadOnlyList<ProfileRow> Rows { get; set; } = Array.Empty<ProfileRow>();

    public async Task OnGetAsync()
    {
        // Gather profiles from every game plugin, then enrich with the core card.
        var rows = new List<ProfileRow>();
        var cards = StellaCardProviderRegistry.Current;
        foreach (var plugin in _plugins.LoadedPlugins.OfType<IStellaGamePlugin>())
        {
            if (plugin.PluginConfig?.Enabled == false) continue;
            foreach (var s in await plugin.GetProfileSummariesAsync())
            {
                var card = cards is null ? null : await cards.GetCardAsync(s.RefId);
                rows.Add(new ProfileRow(
                    s.RefId, s.Name, s.Code, s.Version, s.PluginId ?? plugin.Name,
                    card?.CardId, card?.Paseli ?? 0));
            }
        }
        Rows = rows.OrderBy(r => r.Name).ToList();
    }

    public sealed record ProfileRow(string RefId, string Name, string? Code, int Version, string PluginId,
        string? CardId, int Paseli);
}
