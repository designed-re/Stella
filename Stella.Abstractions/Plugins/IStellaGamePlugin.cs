using System.Collections.Generic;
using System.Threading.Tasks;
using Stella.Abstractions.WebUI;

namespace Stella.Abstractions.Plugins;

/// <summary>
/// Marker/extension contract for plugins that own player profiles (one per
/// game, e.g. KFC). CorePlugin and other non-game plugins do not implement
/// this. The core WebUI uses it to build the cross-plugin profile list and
/// profile-detail views without hardcoding any game.
/// </summary>
public interface IStellaGamePlugin : IStellaPlugin
{
    /// <summary>Summary row for the global profiles list.</summary>
    Task<IReadOnlyList<WebUIProfileSummary>> GetProfileSummariesAsync();

    /// <summary>Per-game profile detail payload for a refid, or null if absent.</summary>
    Task<WebUIProfileDetail?> GetProfileDetailAsync(string refid);
}

/// <summary>Lightweight profile row for the global list.</summary>
public sealed class WebUIProfileSummary
{
    public required string RefId { get; init; }
    public required string Name { get; init; }
    public string? Code { get; init; }
    public int Version { get; init; }
    public string? PluginId { get; init; }
}

/// <summary>Profile detail payload rendered on the profile page.</summary>
public sealed class WebUIProfileDetail
{
    public required string RefId { get; init; }
    public required string Name { get; init; }
    public string? Code { get; init; }
    public int Version { get; init; }
    public string? PluginId { get; init; }
    public IReadOnlyDictionary<string, string?> Extra { get; init; } = new Dictionary<string, string?>();
}
