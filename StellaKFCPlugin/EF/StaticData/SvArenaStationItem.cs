using System;

namespace StellaKFCPlugin.EF.StaticData;

/// <summary>
/// Arena station items (asphyxia ARENA_STATION_ITEMS / ARENA_STATION_ITEMS7).
/// </summary>
public partial class SvArenaStationItem
{
    public int Id { get; set; }

    public int Version { get; set; }

    public string SetName { get; set; } = null!;

    public int MinVersion { get; set; }

    /// <summary>JSON-serialised tuple array: [catalog_id, catalog_type, price, item_type, item_id, param].</summary>
    public string ItemsJson { get; set; } = null!;
}