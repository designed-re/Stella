using System;

namespace StellaKFCPlugin.EF;

/// <summary>
/// Data store(Arena) for Sound Voltex - per season arena progress.
/// </summary>
public partial class SvArena
{
    public int Id { get; set; }

    public int Profile { get; set; }

    public int Season { get; set; }

    public int Version { get; set; }

    public int RankPoint { get; set; }

    public int ShopPoint { get; set; }

    public int UltimateRate { get; set; }

    public int UltimateRankNum { get; set; }

    public int MegamixRate { get; set; }

    public int RankCount { get; set; }

    public int UltimateCount { get; set; }

    public int LiveEnergy { get; set; }

    public virtual SvProfile ProfileNavigation { get; set; } = null!;
}