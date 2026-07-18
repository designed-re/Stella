namespace StellaKFCPlugin.EF.StaticData;

/// <summary>
/// Event list entries (asphyxia webui/asset/json/events.json events6/events7).
/// Each entry describes an in-game event (stamp/completestamp/tama/variant/
/// achmissions/cross_online/gift/...). The <see cref="Enabled"/> flag is the
/// user toggle (asphyxia webui/asset/config/events.json [id].toggle) — it
/// defaults to false so, like asphyxia with no config file, no event extends
/// are emitted until a user opts in via the WebUI.
/// </summary>
public partial class SvEventList
{
    public int Id { get; set; }

    public int Version { get; set; }

    /// <summary>Event id (e.g. sdvx10thstamp, vgate1, achmissions).</summary>
    public string EventId { get; set; } = null!;

    /// <summary>Event type (stamp/completestamp/tama/variant/achmissions/cross_online/gift/...).</summary>
    public string Type { get; set; } = null!;

    /// <summary>Minimum datecode (first array element when asphyxia ships an array).</summary>
    public int MinVersion { get; set; }

    /// <summary>Start date YYYYMMDD (first array element when asphyxia ships an array).</summary>
    public int StartDate { get; set; }

    /// <summary>User toggle (asphyxia config [id].toggle). False by default.</summary>
    public bool Enabled { get; set; }

    /// <summary>Display name (WebUI only).</summary>
    public string? Name { get; set; }

    /// <summary>
    /// JSON-serialised per-event settings (asphyxia config [id].settings), e.g.
    /// variant gate {minOverTrackRank, minSealDiff}.
    /// </summary>
    public string? SettingsJson { get; set; }
}
