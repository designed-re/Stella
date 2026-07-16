using System;

namespace StellaKFCPlugin.EF.StaticData;

/// <summary>
/// Megamix song list per version (asphyxia MEGAMIX_SONGS).
/// </summary>
public partial class SvMegamixData
{
    public int Id { get; set; }

    public int Version { get; set; }

    public int MegamixNo { get; set; }

    /// <summary>Comma-separated music ids.</summary>
    public string SongIds { get; set; } = null!;
}