using System;

namespace StellaKFCPlugin.EF.StaticData;

/// <summary>
/// Unlock event definitions per version (asphyxia UNLOCK_EVENTS6/UNLOCK_EVENTS7).
/// </summary>
public partial class SvUnlockEventData
{
    public int Id { get; set; }

    public int Version { get; set; }

    /// <summary>Event id string (e.g. stamp id, completestamp id, tama id, variant id, gift_crew, achmissions).</summary>
    public string EventId { get; set; } = null!;

    /// <summary>Event type: stamp, completestamp, tama, variant, gift_crew, gift_ap, gift, cross_online, achmissions.</summary>
    public string Type { get; set; } = null!;

    /// <summary>Minimum datecode for this event to be active.</summary>
    public int MinVersion { get; set; }

    /// <summary>Start date (YYYYMMDD) or 0 for no date check.</summary>
    public int StartDate { get; set; }

    /// <summary>JSON-serialised event payload (stmpData, refillStamps flag, etc.).</summary>
    public string DataJson { get; set; } = string.Empty;

    /// <summary>JSON-serialised item list for gift-type events (asphyxia EVENT_ITEMS).</summary>
    public string? ItemsJson { get; set; }

    /// <summary>JSON-serialised toggle list for achmissions (asphyxia toggle keys).</summary>
    public string? TogglesJson { get; set; }

    /// <summary>JSON-serialised settings (e.g. variant gate minOverTrackRank/minSealDiff).</summary>
    public string? SettingsJson { get; set; }
}