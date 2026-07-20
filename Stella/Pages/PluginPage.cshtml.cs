using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Stella.Abstractions.Plugins;
using Stella.Services;

namespace Stella.WebUI.Pages;

[Authorize]
public class PluginPageModel : PageModel
{
    private readonly PluginService _plugins;
    public PluginPageModel(PluginService plugins) => _plugins = plugins;

    public IHtmlContent Body { get; set; } = HtmlString.Empty;
    public string Title { get; set; } = "";

    public async Task<IActionResult> OnGetAsync(string pluginId, string? slug)
    {
        var plugin = _plugins.LoadedPlugins.FirstOrDefault(p =>
            string.Equals(p.Name, pluginId, StringComparison.OrdinalIgnoreCase));
        if (plugin is null || plugin.PluginConfig?.Enabled == false) return NotFound();

        var page = plugin.WebUIPages.FirstOrDefault(p => p.Slug == slug);
        if (page is null) return NotFound();
        Title = $"{pluginId} — {page.Title}";

        var html = await plugin.RenderWebUIPageAsync(page.Slug, HttpContext.RequestServices);
        Body = html is null ? new HtmlString("<p class=\"text-zinc-500\">This page is not implemented yet.</p>") : new HtmlString(html);
        return Page();
    }
}
