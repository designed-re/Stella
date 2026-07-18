namespace StellaKFCPlugin.EF.StaticData;

/// <summary>
/// Startup flag toggles (asphyxia webui/asset/json/events.json <c>flags</c>
/// array, managed by the <c>manageStartupFlags</c> WebUI event which writes
/// <c>webui/asset/config/flags.json</c>). Each entry has a display key, an
/// explanation, and one or more event-string values that are pushed into the
/// <c>common</c> response <c>event.info</c> list when <see cref="Enabled"/> is
/// true (asphyxia common.ts L57-77 / L85-99).
/// </summary>
public partial class SvStartupFlag
{
    public int Id { get; set; }

    /// <summary>Flag id (asphyxia events.json flags[].id, e.g. cuda, submonvsync).</summary>
    public string FlagId { get; set; } = null!;

    /// <summary>Display label (asphyxia events.json flags[].key).</summary>
    public string DisplayName { get; set; } = null!;

    /// <summary>JSON array of event strings (asphyxia flags[].str, string or string[]).</summary>
    public string EventStringsJson { get; set; } = "[]";

    /// <summary>User toggle (asphyxia flags.json [id].toggle). False by default.</summary>
    public bool Enabled { get; set; }
}
