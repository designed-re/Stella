using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Stella.Abstractions.Cards;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Stella.Abstractions.Plugins;
using Stella.Services;

namespace Stella.WebUI.Pages;

[Authorize]
public class ProfileModel : PageModel
{
    private readonly PluginService _plugins;
    public ProfileModel(PluginService plugins) => _plugins = plugins;

    public string RefId { get; set; } = "";
    public string? CardId { get; set; }
    public int Paseli { get; set; }
    public string? PassCode { get; set; }
    public IReadOnlyList<TabRow> Tabs { get; set; } = Array.Empty<TabRow>();

    public async Task<IActionResult> OnGetAsync(string refid)
    {
        RefId = refid ?? "";
        var cards = StellaCardProviderRegistry.Current;
        var card = cards is null ? null : await cards.GetCardAsync(RefId);
        CardId = card?.CardId;
        Paseli = card?.Paseli ?? 0;
        PassCode = card?.PassCode;

        var tabs = new List<TabRow>();
        foreach (var plugin in _plugins.LoadedPlugins.OfType<IStellaGamePlugin>())
        {
            if (plugin.PluginConfig?.Enabled == false) continue;
            var detail = await plugin.GetProfileDetailAsync(RefId);
            if (detail is null) continue;
            foreach (var page in plugin.ProfilePages)
            {
                tabs.Add(new TabRow(plugin.Name, page.Slug, page.Title, detail.Name, detail.Code, detail.Version));
            }
        }
        Tabs = tabs;
        return Page();
    }

    public sealed record TabRow(string PluginId, string Slug, string Title, string Name, string? Code, int Version);
}
