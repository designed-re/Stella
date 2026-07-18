namespace Stella.Abstractions.WebUI;

/// <summary>
/// Describes a single WebUI page contributed by a plugin. Mirrors asphyxia's
/// per-plugin <c>webui/*.pug</c> pages. The host renders a sidebar entry per
/// descriptor and routes <c>/webui/&lt;pluginId&gt;/&lt;Slug&gt;</c> to the
/// plugin's Razor page.
/// </summary>
public sealed class WebUIPageDescriptor
{
    /// <summary>Display title in the sidebar.</summary>
    public required string Title { get; init; }

    /// <summary>URL slug, lowercase kebab-case (e.g. "songs-list").</summary>
    public required string Slug { get; init; }

    /// <summary>
    /// View file name under the plugin's <c>Views/Pages/</c> folder
    /// (e.g. "Data.cshtml"). Defaults to a PascalCase of <see cref="Slug"/> + ".cshtml".
    /// </summary>
    public string View { get; init; } = "";

    /// <summary>Optional icon name (Material Design Icons, without the "mdi-" prefix).</summary>
    public string? Icon { get; init; }
}

/// <summary>Group a page belongs to.</summary>
public enum WebUIPageGroup
{
    /// <summary>Top-level plugin page (asphyxia non-<c>profile_</c> pug).</summary>
    TopLevel,

    /// <summary>Profile-detail tab page (asphyxia <c>profile_</c> pug).</summary>
    ProfileTab,
}
