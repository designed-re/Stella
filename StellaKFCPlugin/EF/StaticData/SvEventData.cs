using System;

namespace StellaKFCPlugin.EF.StaticData;

/// <summary>
/// Static event flags per version (asphyxia EVENT6/EVENT7 arrays).
/// </summary>
public partial class SvEventData
{
    public int Id { get; set; }

    public int Version { get; set; }

    public int SortOrder { get; set; }

    /// <summary>Event flag string (may contain tab-separated params).</summary>
    public string EventId { get; set; } = null!;

    /// <summary>If non-zero, only enabled when toggle key matches this id (asphyxia flags.json).</summary>
    public string? ToggleKey { get; set; }
}