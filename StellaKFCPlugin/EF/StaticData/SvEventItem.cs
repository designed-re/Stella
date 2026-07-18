namespace StellaKFCPlugin.EF.StaticData;

/// <summary>
/// Event reward item lists (asphyxia EVENT_ITEMS6/EVENT_ITEMS7). Keyed by
/// event id (boolean-toggle/direct events) or &lt;eventId&gt;_&lt;idx&gt;
/// (object-toggle/prefix events). Each value is a JSON array of item ids
/// granted as presents by the load handler when the event is toggled on.
/// </summary>
public partial class SvEventItem
{
    public int Id { get; set; }

    public int Version { get; set; }

    /// <summary>Event item key (event id or eventId_idx).</summary>
    public string ItemKey { get; set; } = null!;

    /// <summary>JSON array of item ids, e.g. ["1737"].</summary>
    public string ItemsJson { get; set; } = "[]";
}
