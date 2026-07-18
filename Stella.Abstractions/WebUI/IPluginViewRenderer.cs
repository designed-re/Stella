using System.Threading.Tasks;

namespace Stella.Abstractions.WebUI;

/// <summary>
/// Renders a Razor view (by virtual path) to an HTML string. Implemented by the
/// host (<c>Stella.WebUI.PluginViewRenderer</c>) and consumed by plugins so a
/// plugin can render its embedded page views without a host project reference.
/// </summary>
public interface IPluginViewRenderer
{
    /// <summary>Renders <paramref name="viewPath"/> (e.g. /Plugins/MyPlugin/Pages/View.cshtml) with the given model.</summary>
    Task<string> RenderAsync<TModel>(string viewPath, TModel model);
}
