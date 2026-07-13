using System;

namespace StellaKFCPlugin.EF;

/// <summary>
/// Data store(Variant Power) for Sound Voltex - VARIANT GATE radar progress.
/// </summary>
public partial class SvVariantPower
{
    public int Id { get; set; }

    public int Profile { get; set; }

    public int Version { get; set; }

    public int Power { get; set; }

    public int Notes { get; set; }

    public int Peak { get; set; }

    public int Tsumami { get; set; }

    public int Tricky { get; set; }

    public int Onehand { get; set; }

    public int Handtrip { get; set; }

    /// <summary>Space-separated integers (JSON-serialised for DB storage).</summary>
    public string OverRadar { get; set; } = string.Empty;

    public virtual SvProfile ProfileNavigation { get; set; } = null!;
}