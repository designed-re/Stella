using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stella.Abstractions.Plugins;
using Stella.Services;

namespace Stella.WebUI.Pages;

[Authorize]
public class IndexModel : PageModel
{
    private readonly PluginService _plugins;
    public IndexModel(PluginService plugins) => _plugins = plugins;

    public string Memory { get; set; } = "";
    public IReadOnlyList<PluginRow> PluginRows { get; set; } = Array.Empty<PluginRow>();

    public void OnGet()
    {
        Memory = $"{Math.Round(System.Diagnostics.Process.GetCurrentProcess().WorkingSet64 / 1048576.0, 1)} MB";
        PluginRows = _plugins.LoadedPlugins
            .Select(p => new PluginRow(p.Name, p.Version, p.GameCode, p.Description,
                p.PluginConfig?.Enabled ?? false,
                p.WebUIPages.Count,
                p.ProfilePages.Count))
            .ToList();
    }

    public sealed record PluginRow(string Name, string Version, string GameCode, string Description,
        bool Enabled, int WebUIPages, int ProfilePages);
}
